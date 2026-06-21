using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.EventHandlers;

public sealed class SubscriptionCreatedHandler : INotificationHandler<SubscriptionCreatedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IRealtimeService _realtimeService;

    public SubscriptionCreatedHandler(INotificationService notificationService, IRealtimeService realtimeService)
    {
        _notificationService = notificationService;
        _realtimeService = realtimeService;
    }

    public async Task Handle(SubscriptionCreatedEvent notification, CancellationToken cancellationToken)
    {
        await _notificationService.SendNotificationToGroupAsync(
            "Admins",
            "New Subscription",
            $"Subscription #{notification.SubscriptionId} created",
            NotificationType.Info);

        await _realtimeService.SubscriptionUpdated(notification.SubscriptionId);
    }
}
