using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Helpers;

namespace SportAcademy.Application.EventHandlers;

public sealed class TraineeGroupCreatedHandler(INotificationService notificationService)
    : INotificationHandler<TraineeGroupCreatedEvent>
{
    public async Task Handle(TraineeGroupCreatedEvent notification, CancellationToken cancellationToken)
    {
        await notificationService.SendNotificationToGroupsAsync(
            [NotificationGroupNames.Employees],
            "New Trainee Group",
            $"Trainee group \"{notification.TraineeGroupName}\" was created by {notification.ActorName}",
            NotificationType.Info);
    }
}
