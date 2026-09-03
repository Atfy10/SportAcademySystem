using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

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

        await notificationService.SendNotificationToGroupAsync(
            "Admins",
            "New Excuse Request",
            $"Excuse request #{notification.ExcuseRequestId} filed by {actorName} needs approval",
            NotificationType.Attendance);

        await realtimeService.ExcuseRequestQueueUpdated();
    }
}
