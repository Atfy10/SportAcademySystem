using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.EventHandlers;

public sealed class ExcuseRequestReviewedHandler(
    INotificationService notificationService,
    IExcuseRequestRepository excuseRequestRepository)
    : INotificationHandler<ExcuseRequestReviewedEvent>
{
    public async Task Handle(ExcuseRequestReviewedEvent notification, CancellationToken cancellationToken)
    {
        var excuseRequest = await excuseRequestRepository.GetByIdAsync(notification.ExcuseRequestId, cancellationToken);
        if (excuseRequest?.RequestedByUserId is null) return;

        var title = notification.Approved ? "Excuse Request Approved" : "Excuse Request Rejected";
        var message = notification.Approved
            ? $"Excuse request #{notification.ExcuseRequestId} was approved."
            : $"Excuse request #{notification.ExcuseRequestId} was rejected.";

        await notificationService.SendNotificationAsync(
            excuseRequest.RequestedByUserId,
            title,
            message,
            NotificationType.Attendance);
    }
}
