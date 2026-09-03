using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.EventHandlers;

public sealed class ExcuseRequestReviewedHandler(
    INotificationService notificationService,
    IRealtimeService realtimeService,
    IUserRepository userRepository,
    IExcuseRequestRepository excuseRequestRepository)
    : INotificationHandler<ExcuseRequestReviewedEvent>
{
    public async Task Handle(ExcuseRequestReviewedEvent notification, CancellationToken cancellationToken)
    {
        await realtimeService.ExcuseRequestQueueUpdated();

        var excuseRequest = await excuseRequestRepository.GetByIdAsync(notification.ExcuseRequestId, cancellationToken);
        if (excuseRequest?.RequestedByUserId is null) return;

        // Reads the reviewer from the entity's own persisted ReviewedByUserId (already loaded
        // above) rather than the ambient IUserContextService - same reasoning as
        // ExcuseRequestCreatedHandler: the DB fact can't drift from what
        // Approve/RejectExcuseRequestCommandHandler actually wrote.
        var actorName = excuseRequest.ReviewedByUserId is { } reviewedByRaw && Guid.TryParse(reviewedByRaw, out var reviewedBy)
            ? await userRepository.GetDisplayNameAsync(reviewedBy, cancellationToken)
            : "System";

        var title = notification.Approved ? "Excuse Request Approved" : "Excuse Request Rejected";
        var message = notification.Approved
            ? $"Excuse request #{notification.ExcuseRequestId} was approved by {actorName}."
            : $"Excuse request #{notification.ExcuseRequestId} was rejected by {actorName}.";

        await notificationService.SendNotificationAsync(
            excuseRequest.RequestedByUserId,
            title,
            message,
            NotificationType.Attendance);
    }
}
