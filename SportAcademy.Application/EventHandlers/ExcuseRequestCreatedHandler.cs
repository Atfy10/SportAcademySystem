using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.EventHandlers;

public sealed class ExcuseRequestCreatedHandler(
    INotificationService notificationService,
    IRealtimeService realtimeService,
    IUserContextService userContext,
    IUserRepository userRepository)
    : INotificationHandler<ExcuseRequestCreatedEvent>
{
    public async Task Handle(ExcuseRequestCreatedEvent notification, CancellationToken cancellationToken)
    {
        var actorName = userContext.UserId is { } userId
            ? await userRepository.GetDisplayNameAsync(userId, cancellationToken)
            : "System";

        await notificationService.SendNotificationToGroupAsync(
            "Admins",
            "New Excuse Request",
            $"Excuse request #{notification.ExcuseRequestId} filed by {actorName} needs approval",
            NotificationType.Attendance);

        await realtimeService.ExcuseRequestQueueUpdated();
    }
}
