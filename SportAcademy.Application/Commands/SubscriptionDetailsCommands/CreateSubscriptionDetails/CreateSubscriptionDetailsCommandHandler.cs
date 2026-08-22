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
        private readonly IUserContextService _userContext;
        private readonly IMapper _mapper;
        private readonly IPublisher _publisher;

        public CreateSubscriptionDetailsCommandHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            SubDetailsManagementService subscriptionDetailsMangeService,
            ISportPriceRepository sportPriceRepository,
            IFinanceLedgerService financeLedgerService,
            ITraineeRepository traineeRepository,
            IUserContextService userContext,
            IMapper mapper,
            IPublisher publisher)
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
            _subscriptionDetailsMangeService = subscriptionDetailsMangeService;
            _sportPriceRepository = sportPriceRepository;
            _financeLedgerService = financeLedgerService;
            _traineeRepository = traineeRepository;
            _userContext = userContext;
            _mapper = mapper;
            _publisher = publisher;
        }

        public async Task<Result<int>> Handle(CreateSubscriptionDetailsCommand request, CancellationToken cancellationToken)
        {
            var sportPrice = await _sportPriceRepository.GetByKeyAsync(
                request.BranchId, request.SportId, request.SubscriptionTypeId, cancellationToken)
                ?? throw new IdNotFoundException(nameof(SportPrice), $"{request.BranchId}/{request.SportId}/{request.SubscriptionTypeId}");

            var subDetails = _mapper.Map<SubscriptionDetails>(request)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            await _subscriptionDetailsMangeService
                .ValidateSubscriptionAsync(subDetails, cancellationToken);

            var isSubActive = SubscriptionDetailsService.IsSubscriptionActive(subDetails);
            if (!isSubActive)
                subDetails.Status = SubscriptionStatus.Expired;

            cancellationToken.ThrowIfCancellationRequested();

            await _subscriptionDetailsRepository.AddAsync(subDetails, cancellationToken);

            // Subscriptions are typically paid for at the point of sale (a parent registering
            // and paying the same day), so creation issues an Invoice and immediately records a
            // full payment against it via the chosen method - not a deferred Accountant-only
            // step. Recording additional/partial payments later still goes through the
            // Accountant console (see FinanceController.RecordPayment).
            var invoice = await _financeLedgerService.IssueSubscriptionInvoiceAsync(
                subDetails, sportPrice.Price, "KWD", cancellationToken);

            await _financeLedgerService.RecordPaymentAsync(new RecordPaymentInput(
                Amount: sportPrice.Price,
                Method: request.PaymentMethod,
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
