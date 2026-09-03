using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.EventHandlers;

public sealed class UserRolesChangedHandler(INotificationService notificationService)
    : INotificationHandler<UserRolesChangedEvent>
{
    public async Task Handle(UserRolesChangedEvent notification, CancellationToken cancellationToken)
    {
        await notificationService.SendNotificationAsync(
            notification.UserId.ToString(),
            "Your Roles Were Updated",
            $"Your account roles/permissions were changed by {notification.ActorName}.",
            NotificationType.Warning);
    }
}
