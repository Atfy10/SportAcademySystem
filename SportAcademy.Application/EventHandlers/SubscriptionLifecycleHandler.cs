using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Helpers;

namespace SportAcademy.Application.EventHandlers;

public sealed class SubscriptionLifecycleHandler(INotificationService notificationService)
    : INotificationHandler<SubscriptionLifecycleEvent>
{
    public async Task Handle(SubscriptionLifecycleEvent notification, CancellationToken cancellationToken)
    {
        await notificationService.SendNotificationToGroupsAsync(
            [NotificationGroupNames.Admins, NotificationGroupNames.Owners],
            $"Subscription {notification.Action}",
            $"Subscription #{notification.SubscriptionId} was {notification.Action.ToLowerInvariant()} by {notification.ActorName}",
            NotificationType.Warning);
    }
}
