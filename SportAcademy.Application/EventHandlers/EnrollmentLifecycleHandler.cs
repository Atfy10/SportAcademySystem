using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Helpers;

namespace SportAcademy.Application.EventHandlers;

public sealed class EnrollmentLifecycleHandler(INotificationService notificationService)
    : INotificationHandler<EnrollmentLifecycleEvent>
{
    public async Task Handle(EnrollmentLifecycleEvent notification, CancellationToken cancellationToken)
    {
        await notificationService.SendNotificationToGroupsAsync(
            [NotificationGroupNames.Admins, NotificationGroupNames.Owners],
            $"Enrollment {notification.Action}",
            $"Enrollment #{notification.EnrollmentId} was {notification.Action.ToLowerInvariant()} by {notification.ActorName}",
            NotificationType.Enrollment);
    }
}
