using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.EventHandlers;

public sealed class ExcuseRequestCreatedHandler(INotificationService notificationService)
    : INotificationHandler<ExcuseRequestCreatedEvent>
{
    public async Task Handle(ExcuseRequestCreatedEvent notification, CancellationToken cancellationToken)
    {
        await notificationService.SendNotificationToGroupAsync(
            "Admins",
            "New Excuse Request",
            $"Excuse request #{notification.ExcuseRequestId} needs approval",
            NotificationType.Attendance);
    }
}
