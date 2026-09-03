using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Helpers;

namespace SportAcademy.Application.EventHandlers;

public sealed class EmployeeStatusChangedHandler(INotificationService notificationService)
    : INotificationHandler<EmployeeStatusChangedEvent>
{
    public async Task Handle(EmployeeStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        var status = notification.IsWork ? "reactivated" : "deactivated";
        await notificationService.SendNotificationToGroupsAsync(
            [NotificationGroupNames.Admins, NotificationGroupNames.Owners],
            "Employee Status Changed",
            $"Employee #{notification.EmployeeId} was {status} by {notification.ActorName}",
            NotificationType.Info);
    }
}
