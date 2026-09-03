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
        if (excuseRequest is null) return;

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

        // This is an org-wide outcome announcement, not just a reply to whoever filed it -
        // every staff member (coaches, accountants, managers, admins - anyone with an Employee
        // record) plus the Owner gets it, in addition to the original requester (already
        // included whenever they're staff themselves, but unioned in explicitly in case they
        // aren't linked as an Employee for some reason).
        var recipientIds = await userRepository.GetStaffAndOwnerUserIdsAsync(cancellationToken);
        if (Guid.TryParse(excuseRequest.RequestedByUserId, out var requesterId))
        {
            recipientIds.Add(requesterId);
        }

        await notificationService.SendNotificationToUsersAsync(
            recipientIds,
            title,
            message,
            NotificationType.Attendance);
    }
}
