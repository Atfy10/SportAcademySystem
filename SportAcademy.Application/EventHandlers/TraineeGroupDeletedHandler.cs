using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Helpers;

namespace SportAcademy.Application.EventHandlers;

public sealed class TraineeGroupDeletedHandler(INotificationService notificationService)
    : INotificationHandler<TraineeGroupDeletedEvent>
{
    public async Task Handle(TraineeGroupDeletedEvent notification, CancellationToken cancellationToken)
    {
        await notificationService.SendNotificationToGroupsAsync(
            [NotificationGroupNames.Admins, NotificationGroupNames.Owners],
            "Trainee Group Deleted",
            $"Trainee group \"{notification.TraineeGroupName}\" was deleted by {notification.ActorName}",
            NotificationType.Warning);
    }
}
