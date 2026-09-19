using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Helpers;

namespace SportAcademy.Application.EventHandlers;

public sealed class UserRolesChangedHandler(INotificationService notificationService)
    : INotificationHandler<UserRolesChangedEvent>
{
    public async Task Handle(UserRolesChangedEvent notification, CancellationToken cancellationToken)
    {
        await notificationService.SendNotificationAsync(
            NotificationEventTypes.UserRolesChanged,
            notification.UserId.ToString(),
            "Your Roles Were Updated",
            $"Your account roles/permissions were changed by {notification.ActorName}.",
            NotificationType.Warning);
    }
}
