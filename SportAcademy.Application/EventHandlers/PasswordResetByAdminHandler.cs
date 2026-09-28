using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Helpers;

namespace SportAcademy.Application.EventHandlers;

public sealed class PasswordResetByAdminHandler(INotificationService notificationService)
    : INotificationHandler<PasswordResetByAdminEvent>
{
    public async Task Handle(PasswordResetByAdminEvent notification, CancellationToken cancellationToken)
    {
        await notificationService.SendNotificationAsync(
            NotificationEventTypes.PasswordResetByAdmin,
            notification.TargetUserId.ToString(),
            "Password Reset",
            $"Your password was reset by {notification.ActorName}. If this wasn't expected, contact your administrator.",
            NotificationType.Warning);
    }
}
