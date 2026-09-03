using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Helpers;

namespace SportAcademy.Application.EventHandlers;

public sealed class EmployeeLifecycleHandler(INotificationService notificationService)
    : INotificationHandler<EmployeeLifecycleEvent>
{
    public async Task Handle(EmployeeLifecycleEvent notification, CancellationToken cancellationToken)
    {
        await notificationService.SendNotificationToGroupsAsync(
            [NotificationGroupNames.Admins, NotificationGroupNames.Owners],
            $"Employee {notification.Action}",
            $"Employee \"{notification.EmployeeName}\" was {notification.Action.ToLowerInvariant()} by {notification.ActorName}",
            NotificationType.Info);
    }
}
