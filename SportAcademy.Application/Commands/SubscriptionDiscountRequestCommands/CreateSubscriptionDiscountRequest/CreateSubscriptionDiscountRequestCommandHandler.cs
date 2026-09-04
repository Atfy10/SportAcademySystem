using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDiscountRequestDtos;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.DiscountCodeExceptions;

namespace SportAcademy.Application.Commands.SubscriptionDiscountRequestCommands.CreateSubscriptionDiscountRequest
{
    // Does NOT create any SubscriptionDetails/Invoice/Payment - it only validates the code (so an
    // obviously-bad one fails fast instead of sitting in the approval queue) and queues the
    // request as PendingApproval. Actual creation happens in
    // ApproveSubscriptionDiscountRequestCommandHandler.
    public class CreateSubscriptionDiscountRequestCommandHandler
        : IRequestHandler<CreateSubscriptionDiscountRequestCommand, Result<SubscriptionDiscountRequestDto>>
    {
        private readonly string _operation = OperationType.Add.ToString();
        private readonly ISubscriptionDiscountRequestRepository _repository;
        private readonly IDiscountCodeRepository _discountCodeRepository;
        private readonly IUserContextService _userContext;
        private readonly IUserRepository _userRepository;
        private readonly IPublisher _publisher;

        public CreateSubscriptionDiscountRequestCommandHandler(
            ISubscriptionDiscountRequestRepository repository,
            IDiscountCodeRepository discountCodeRepository,
            IUserContextService userContext,
            IUserRepository userRepository,
            IPublisher publisher)
        {
            _repository = repository;
            _discountCodeRepository = discountCodeRepository;
            _userContext = userContext;
            _userRepository = userRepository;
            _publisher = publisher;
        }

        public async Task<Result<SubscriptionDiscountRequestDto>> Handle(
            CreateSubscriptionDiscountRequestCommand request, CancellationToken cancellationToken)
        {
            var normalized = request.DiscountCode.Trim().ToUpperInvariant();
            _ = await _discountCodeRepository.GetActiveByCodeAsync(normalized, cancellationToken)
                ?? throw new InvalidDiscountCodeException(request.DiscountCode);

            var requestedByUserId = _userContext.UserId ?? Guid.Empty;

            var entity = new SubscriptionDiscountRequest
            {
                TraineeId = request.TraineeId,
                SubscriptionTypeId = request.SubscriptionTypeId,
                SportId = request.SportId,
                BranchId = request.BranchId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                PaymentTypeId = request.PaymentTypeId,
                DiscountCode = normalized,
                Status = SubscriptionDiscountRequestStatus.PendingApproval,
                RequestedByUserId = requestedByUserId,
                RequestedAt = DateTime.UtcNow,
            };

            await _repository.AddAsync(entity, cancellationToken);
            await _publisher.Publish(new SubscriptionDiscountRequestCreatedEvent(entity.Id), cancellationToken);

            var saved = await _repository.GetByIdWithIncludesAsync(entity.Id, cancellationToken) ?? entity;
            var nameLookup = new Dictionary<Guid, string>
            {
                [requestedByUserId] = await _userRepository.GetDisplayNameAsync(requestedByUserId, cancellationToken),
            };

            return Result<SubscriptionDiscountRequestDto>.Success(
                SubscriptionDiscountRequestMapper.ToDto(saved, nameLookup), _operation);
        }
    }
}
