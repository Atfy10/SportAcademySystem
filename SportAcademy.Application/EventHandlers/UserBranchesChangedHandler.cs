using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.EventHandlers;

public sealed class UserBranchesChangedHandler(INotificationService notificationService)
    : INotificationHandler<UserBranchesChangedEvent>
{
    public async Task Handle(UserBranchesChangedEvent notification, CancellationToken cancellationToken)
    {
        await notificationService.SendNotificationAsync(
            notification.UserId.ToString(),
            "Branch Access Updated",
            $"Your branch access was updated by {notification.ActorName}.",
            NotificationType.Warning);
    }
}
