using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDiscountRequestDtos;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.DiscountCodeExceptions;
using SportAcademy.Domain.Exceptions.SubscriptionDiscountRequestExceptions;

namespace SportAcademy.Application.Commands.SubscriptionDiscountRequestCommands.ApproveSubscriptionDiscountRequest
{
    public class ApproveSubscriptionDiscountRequestCommandHandler
        : IRequestHandler<ApproveSubscriptionDiscountRequestCommand, Result<SubscriptionDiscountRequestDto>>
    {
        private readonly string _operation = OperationType.Update.ToString();
        private readonly ISubscriptionDiscountRequestRepository _repository;
        private readonly IDiscountCodeRepository _discountCodeRepository;
        private readonly ISubscriptionCreationService _subscriptionCreationService;
        private readonly IUserContextService _userContext;
        private readonly IUserRepository _userRepository;
        private readonly IPublisher _publisher;

        public ApproveSubscriptionDiscountRequestCommandHandler(
            ISubscriptionDiscountRequestRepository repository,
            IDiscountCodeRepository discountCodeRepository,
            ISubscriptionCreationService subscriptionCreationService,
            IUserContextService userContext,
            IUserRepository userRepository,
            IPublisher publisher)
        {
            _repository = repository;
            _discountCodeRepository = discountCodeRepository;
            _subscriptionCreationService = subscriptionCreationService;
            _userContext = userContext;
            _userRepository = userRepository;
            _publisher = publisher;
        }

        public async Task<Result<SubscriptionDiscountRequestDto>> Handle(
            ApproveSubscriptionDiscountRequestCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new SubscriptionDiscountRequestNotFoundException(request.Id.ToString());

            if (entity.Status != SubscriptionDiscountRequestStatus.PendingApproval)
                throw new InvalidSubscriptionDiscountRequestTransitionException(entity.Id, entity.Status.ToString(), "approved");

            // Re-validate: the code may have expired or been deactivated between request and
            // approval - never trust the value at request time. If it's no longer valid, the
            // approver gets a clear error and must Reject instead.
            var code = await _discountCodeRepository.GetActiveByCodeAsync(entity.DiscountCode, cancellationToken)
                ?? throw new InvalidDiscountCodeException(entity.DiscountCode);

            var subscription = await _subscriptionCreationService.CreateAsync(
                entity.TraineeId, entity.SubscriptionTypeId, entity.SportId, entity.BranchId,
                entity.StartDate, entity.EndDate, entity.PaymentTypeId,
                discountPercentage: code.PercentageOff, discountCodeId: code.Id,
                actingUserId: entity.RequestedByUserId,
                cancellationToken);

            entity.Status = SubscriptionDiscountRequestStatus.Approved;
            entity.ReviewedByUserId = _userContext.UserId;
            entity.ReviewedAt = DateTime.UtcNow;
            entity.CreatedSubscriptionDetailsId = subscription.Id;

            await _repository.UpdateAsync(entity, cancellationToken);
            await _publisher.Publish(new SubscriptionDiscountRequestReviewedEvent(entity.Id, Approved: true), cancellationToken);

            var saved = await _repository.GetByIdWithIncludesAsync(entity.Id, cancellationToken) ?? entity;
            var nameLookup = await BuildNameLookupAsync(saved, cancellationToken);

            return Result<SubscriptionDiscountRequestDto>.Success(
                SubscriptionDiscountRequestMapper.ToDto(saved, nameLookup), _operation);
        }

        private async Task<Dictionary<Guid, string>> BuildNameLookupAsync(
            Domain.Entities.Finance.SubscriptionDiscountRequest r, CancellationToken ct)
        {
            var lookup = new Dictionary<Guid, string>
            {
                [r.RequestedByUserId] = await _userRepository.GetDisplayNameAsync(r.RequestedByUserId, ct),
            };
            if (r.ReviewedByUserId is { } reviewedBy)
                lookup[reviewedBy] = await _userRepository.GetDisplayNameAsync(reviewedBy, ct);
            return lookup;
        }
    }
}
