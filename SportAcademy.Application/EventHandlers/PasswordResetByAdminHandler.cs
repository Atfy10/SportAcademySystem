using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.EventHandlers;

public sealed class PasswordResetByAdminHandler(INotificationService notificationService)
    : INotificationHandler<PasswordResetByAdminEvent>
{
    public async Task Handle(PasswordResetByAdminEvent notification, CancellationToken cancellationToken)
    {
        await notificationService.SendNotificationAsync(
            notification.TargetUserId.ToString(),
            "Password Reset",
            $"Your password was reset by {notification.ActorName}. If this wasn't expected, contact your administrator.",
            NotificationType.Warning);
    }
}
