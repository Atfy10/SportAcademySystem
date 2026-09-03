using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SubscriptonExceptions;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.ActivateSubscription
{
    public class ActivateSubscriptionCommandHandler : IRequestHandler<ActivateSubscriptionCommand, Result<bool>>
    {
        private readonly string _operation = OperationType.Update.ToString();
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;
        private readonly IUserContextService _userContext;
        private readonly IUserRepository _userRepository;
        private readonly IPublisher _publisher;

        public ActivateSubscriptionCommandHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            IUserContextService userContext,
            IUserRepository userRepository,
            IPublisher publisher)
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
            _userContext = userContext;
            _userRepository = userRepository;
            _publisher = publisher;
        }

        public async Task<Result<bool>> Handle(ActivateSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var subDetails = await _subscriptionDetailsRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new SubscriptionDetailsNotFoundException(request.Id.ToString());

            cancellationToken.ThrowIfCancellationRequested();

            subDetails.Status = SubscriptionStatus.Active;

            await _subscriptionDetailsRepository.UpdateAsync(subDetails, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var actorName = _userContext.UserId is { } userId
                ? await _userRepository.GetDisplayNameAsync(userId, cancellationToken)
                : "System";
            await _publisher.Publish(new SubscriptionLifecycleEvent(subDetails.Id, "Activated", actorName), cancellationToken);

            return Result<bool>.Success(true, _operation);
        }
    }
}
