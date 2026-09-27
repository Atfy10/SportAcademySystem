using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Helpers;

namespace SportAcademy.Application.EventHandlers;

public sealed class UserBranchesChangedHandler(INotificationService notificationService)
    : INotificationHandler<UserBranchesChangedEvent>
{
    public async Task Handle(UserBranchesChangedEvent notification, CancellationToken cancellationToken)
    {
        await notificationService.SendNotificationAsync(
            NotificationEventTypes.UserBranchesChanged,
            notification.UserId.ToString(),
            "Branch Access Updated",
            $"Your branch access was updated by {notification.ActorName}.",
            NotificationType.Warning);
    }
}
