using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.EventHandlers;

public sealed class SubscriptionCreatedHandler : INotificationHandler<SubscriptionCreatedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IRealtimeService _realtimeService;
    private readonly IUserContextService _userContext;
    private readonly IUserRepository _userRepository;

    public SubscriptionCreatedHandler(
        INotificationService notificationService,
        IRealtimeService realtimeService,
        IUserContextService userContext,
        IUserRepository userRepository)
    {
        _notificationService = notificationService;
        _realtimeService = realtimeService;
        _userContext = userContext;
        _userRepository = userRepository;
    }

    public async Task Handle(SubscriptionCreatedEvent notification, CancellationToken cancellationToken)
    {
        var actorName = _userContext.UserId is { } userId
            ? await _userRepository.GetDisplayNameAsync(userId, cancellationToken)
            : "System";

        await _notificationService.SendNotificationToGroupAsync(
            "Admins",
            "New Subscription",
            $"Subscription #{notification.SubscriptionId} created by {actorName}",
            NotificationType.Info);

        await _realtimeService.SubscriptionUpdated(notification.SubscriptionId);
    }
}
