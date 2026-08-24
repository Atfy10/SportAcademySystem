using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Services;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.CreateSubscriptionDetails
{
    public class CreateSubscriptionDetailsCommandHandler : IRequestHandler<CreateSubscriptionDetailsCommand, Result<int>>
    {
        private readonly string _operation = OperationType.Add.ToString();
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;
        private readonly SubDetailsManagementService _subscriptionDetailsMangeService;
        private readonly ISportPriceRepository _sportPriceRepository;
        private readonly IFinanceLedgerService _financeLedgerService;
        private readonly ITraineeRepository _traineeRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IUserContextService _userContext;
        private readonly IMapper _mapper;
        private readonly IPublisher _publisher;

        public CreateSubscriptionDetailsCommandHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            SubDetailsManagementService subscriptionDetailsMangeService,
            ISportPriceRepository sportPriceRepository,
            IFinanceLedgerService financeLedgerService,
            ITraineeRepository traineeRepository,
            IEnrollmentRepository enrollmentRepository,
            IUserContextService userContext,
            IMapper mapper,
            IPublisher publisher)
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
            _subscriptionDetailsMangeService = subscriptionDetailsMangeService;
            _sportPriceRepository = sportPriceRepository;
            _financeLedgerService = financeLedgerService;
            _traineeRepository = traineeRepository;
            _enrollmentRepository = enrollmentRepository;
            _userContext = userContext;
            _mapper = mapper;
            _publisher = publisher;
        }

        public async Task<Result<int>> Handle(CreateSubscriptionDetailsCommand request, CancellationToken cancellationToken)
        {
            var sportPrice = await _sportPriceRepository.GetByKeyWithIncludesAsync(
                request.BranchId, request.SportId, request.SubscriptionTypeId, cancellationToken)
                ?? throw new IdNotFoundException(nameof(SportPrice), $"{request.BranchId}/{request.SportId}/{request.SubscriptionTypeId}");

            var subDetails = _mapper.Map<SubscriptionDetails>(request)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            await _subscriptionDetailsMangeService
                .ValidateSubscriptionAsync(subDetails, cancellationToken);

            if (SubscriptionDetailsService.HasExpired(subDetails))
                subDetails.Status = SubscriptionStatus.Expired;

            cancellationToken.ThrowIfCancellationRequested();

            await _subscriptionDetailsRepository.AddAsync(subDetails, cancellationToken);

            // A trainee can only be enrolled in one group per sport - if they already have a
            // group enrollment for this sport (i.e. this is a renewal, not a first-time
            // sign-up), carry that enrollment forward onto the new subscription instead of
            // leaving it pointing at the now-superseded one. No manual re-enrollment step
            // needed. First-time subscriptions (no existing enrollment) are untouched - a
            // trainee is still enrolled into a specific group as a separate, deliberate step.
            var existingEnrollment = await _enrollmentRepository.GetCurrentEnrollmentForSportAsync(
                request.TraineeId, request.SportId, cancellationToken);
            if (existingEnrollment is not null)
            {
                var oldSubscriptionDetailsId = existingEnrollment.SubscriptionDetailsId;

                // Not SubscriptionDetailsService.CalculateAllowedSessions(subDetails) - that
                // reads subDetails.SportPrice.SportSubscriptionType.SubscriptionType, which is
                // null on this freshly-mapped-and-added entity (no navigations loaded/attached).
                // sportPrice was fetched with includes specifically for this.
                existingEnrollment.SubscriptionDetailsId = subDetails.Id;
                existingEnrollment.SessionAllowed = sportPrice.SportSubscriptionType.SubscriptionType.DaysPerMonth;
                existingEnrollment.SessionRemaining = existingEnrollment.SessionAllowed;
                existingEnrollment.ExpiryDate = subDetails.EndDate.ToDateTime(TimeOnly.MinValue);
                existingEnrollment.IsActive = true;
                await _enrollmentRepository.UpdateAsync(existingEnrollment, cancellationToken);

                // Expire the superseded subscription immediately rather than waiting for the
                // lazy status flip in GetSubDetailsStatsAsync - otherwise it keeps showing as
                // Active until something else happens to query subscription stats.
                var oldSubscription = await _subscriptionDetailsRepository.GetByIdAsync(oldSubscriptionDetailsId, cancellationToken);
                if (oldSubscription is not null && oldSubscription.Status != SubscriptionStatus.Expired)
                {
                    oldSubscription.Status = SubscriptionStatus.Expired;
                    await _subscriptionDetailsRepository.UpdateAsync(oldSubscription, cancellationToken);
                }
            }

            // Subscriptions are typically paid for at the point of sale (a parent registering
            // and paying the same day), so creation issues an Invoice and immediately records a
            // full payment against it via the chosen method - not a deferred Accountant-only
            // step. Recording additional/partial payments later still goes through the
            // Accountant console (see FinanceController.RecordPayment).
            var invoice = await _financeLedgerService.IssueSubscriptionInvoiceAsync(
                subDetails, sportPrice.Price, "KWD", cancellationToken);

            await _financeLedgerService.RecordPaymentAsync(new RecordPaymentInput(
                Amount: sportPrice.Price,
                PaymentTypeId: request.PaymentTypeId,
                BranchId: request.BranchId,
                Currency: "KWD",
                Reference: null,
                Notes: null,
                RecordedByUserId: _userContext.UserId,
                Allocations: [new PaymentAllocationInput(invoice.Id, sportPrice.Price)]
            ), cancellationToken);

            await _publisher.Publish(new SubscriptionCreatedEvent(subDetails.Id, subDetails.TraineeId), cancellationToken);

            return Result<int>.Success(subDetails.Id, _operation);
        }
    }
}
