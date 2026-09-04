using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDiscountRequestDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SubscriptionDiscountRequestExceptions;

namespace SportAcademy.Application.Commands.SubscriptionDiscountRequestCommands.RejectSubscriptionDiscountRequest
{
    public class RejectSubscriptionDiscountRequestCommandHandler
        : IRequestHandler<RejectSubscriptionDiscountRequestCommand, Result<SubscriptionDiscountRequestDto>>
    {
        private readonly string _operation = OperationType.Update.ToString();
        private readonly ISubscriptionDiscountRequestRepository _repository;
        private readonly IUserContextService _userContext;
        private readonly IUserRepository _userRepository;

        public RejectSubscriptionDiscountRequestCommandHandler(
            ISubscriptionDiscountRequestRepository repository,
            IUserContextService userContext,
            IUserRepository userRepository)
        {
            _repository = repository;
            _userContext = userContext;
            _userRepository = userRepository;
        }

        public async Task<Result<SubscriptionDiscountRequestDto>> Handle(
            RejectSubscriptionDiscountRequestCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new SubscriptionDiscountRequestNotFoundException(request.Id.ToString());

            if (entity.Status != SubscriptionDiscountRequestStatus.PendingApproval)
                throw new InvalidSubscriptionDiscountRequestTransitionException(entity.Id, entity.Status.ToString(), "rejected");

            entity.Status = SubscriptionDiscountRequestStatus.Rejected;
            entity.ReviewedByUserId = _userContext.UserId;
            entity.ReviewedAt = DateTime.UtcNow;
            entity.RejectionReason = request.RejectionReason;

            await _repository.UpdateAsync(entity, cancellationToken);

            var saved = await _repository.GetByIdWithIncludesAsync(entity.Id, cancellationToken) ?? entity;
            var nameLookup = new Dictionary<Guid, string>
            {
                [saved.RequestedByUserId] = await _userRepository.GetDisplayNameAsync(saved.RequestedByUserId, cancellationToken),
            };
            if (saved.ReviewedByUserId is { } reviewedBy)
                nameLookup[reviewedBy] = await _userRepository.GetDisplayNameAsync(reviewedBy, cancellationToken);

            return Result<SubscriptionDiscountRequestDto>.Success(
                SubscriptionDiscountRequestMapper.ToDto(saved, nameLookup), _operation);
        }
    }
}
