using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.EventHandlers;

public sealed class UserActiveStatusChangedHandler(INotificationService notificationService)
    : INotificationHandler<UserActiveStatusChangedEvent>
{
    public async Task Handle(UserActiveStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        var message = notification.IsBanned
            ? $"Your account was deactivated by {notification.ActorName}."
            : $"Your account was reactivated by {notification.ActorName}.";

        await notificationService.SendNotificationAsync(
            notification.UserId.ToString(),
            "Account Status Changed",
            message,
            NotificationType.Warning);
    }
}
