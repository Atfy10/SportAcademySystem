using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SubscriptonExceptions;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.SuspendSubscription
{
    public class SuspendSubscriptionCommandHandler : IRequestHandler<SuspendSubscriptionCommand, Result<bool>>
    {
        private readonly string _operation = OperationType.Update.ToString();
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;
        private readonly IUserContextService _userContext;
        private readonly IUserRepository _userRepository;
        private readonly IPublisher _publisher;

        public SuspendSubscriptionCommandHandler(
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

        public async Task<Result<bool>> Handle(SuspendSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var subDetails = await _subscriptionDetailsRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new SubscriptionDetailsNotFoundException(request.Id.ToString());

            cancellationToken.ThrowIfCancellationRequested();

            if (subDetails.Status is SubscriptionStatus.Expired)
                return Result<bool>.Failure(_operation, "Expired subscription can not be suspended.");

            subDetails.Status = SubscriptionStatus.Suspended;

            await _subscriptionDetailsRepository.UpdateAsync(subDetails, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var actorName = _userContext.UserId is { } userId
                ? await _userRepository.GetDisplayNameAsync(userId, cancellationToken)
                : "System";
            await _publisher.Publish(new SubscriptionLifecycleEvent(subDetails.Id, "Suspended", actorName), cancellationToken);

            return Result<bool>.Success(true, _operation);
        }
    }
}
