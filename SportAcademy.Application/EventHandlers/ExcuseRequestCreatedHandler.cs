using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Helpers;

namespace SportAcademy.Application.EventHandlers;

public sealed class ExcuseRequestCreatedHandler(
    INotificationService notificationService,
    IRealtimeService realtimeService,
    IUserRepository userRepository,
    IExcuseRequestRepository excuseRequestRepository)
    : INotificationHandler<ExcuseRequestCreatedEvent>
{
    public async Task Handle(ExcuseRequestCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Reads the requester from the entity's own persisted RequestedByUserId rather than the
        // ambient IUserContextService - the entity field is the fact that was actually saved to
        // the DB for this request, so it can't drift from what CreateExcuseRequestCommandHandler
        // wrote even if something about the current request context has since changed.
        var excuseRequest = await excuseRequestRepository.GetByIdAsync(notification.ExcuseRequestId, cancellationToken);
        var actorName = excuseRequest?.RequestedByUserId is { } requestedByRaw && Guid.TryParse(requestedByRaw, out var requestedBy)
            ? await userRepository.GetDisplayNameAsync(requestedBy, cancellationToken)
            : "System";

        // Only whoever can actually act on it - Admins and Owners hold attendance.approve_excuse.
        await notificationService.SendNotificationToGroupsAsync(
            [NotificationGroupNames.Admins, NotificationGroupNames.Owners],
            "New Excuse Request",
            $"Excuse request #{notification.ExcuseRequestId} filed by {actorName} needs approval",
            NotificationType.Attendance);

        await realtimeService.ExcuseRequestQueueUpdated();
    }
}
