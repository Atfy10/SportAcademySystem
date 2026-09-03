using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.EventHandlers;

public sealed class ExcuseRequestReviewedHandler(
    INotificationService notificationService,
    IRealtimeService realtimeService,
    IUserContextService userContext,
    IUserRepository userRepository,
    IExcuseRequestRepository excuseRequestRepository)
    : INotificationHandler<ExcuseRequestReviewedEvent>
{
    public async Task Handle(ExcuseRequestReviewedEvent notification, CancellationToken cancellationToken)
    {
        await realtimeService.ExcuseRequestQueueUpdated();

        var excuseRequest = await excuseRequestRepository.GetByIdAsync(notification.ExcuseRequestId, cancellationToken);
        if (excuseRequest?.RequestedByUserId is null) return;

        var actorName = userContext.UserId is { } userId
            ? await userRepository.GetDisplayNameAsync(userId, cancellationToken)
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
