using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.EventHandlers;

public sealed class SubscriptionDiscountRequestReviewedHandler(
    INotificationService notificationService,
    IUserRepository userRepository,
    ISubscriptionDiscountRequestRepository requestRepository)
    : INotificationHandler<SubscriptionDiscountRequestReviewedEvent>
{
    public async Task Handle(SubscriptionDiscountRequestReviewedEvent notification, CancellationToken cancellationToken)
    {
        var request = await requestRepository.GetByIdWithIncludesAsync(notification.RequestId, cancellationToken);
        if (request is null) return;

        var reviewerName = request.ReviewedByUserId is { } reviewedBy
            ? await userRepository.GetDisplayNameAsync(reviewedBy, cancellationToken)
            : "System";

        var title = notification.Approved ? "Discount Request Approved" : "Discount Request Rejected";
        var message = notification.Approved
            ? $"Your discount code \"{request.DiscountCode}\" request was approved by {reviewerName} - the subscription has been created."
            : $"Your discount code \"{request.DiscountCode}\" request was rejected by {reviewerName}"
              + (string.IsNullOrWhiteSpace(request.RejectionReason) ? "." : $": {request.RejectionReason}");

        // Deliberately the requester only, not a group broadcast (unlike
        // ExcuseRequestReviewedHandler's org-wide announcement) - whether a specific
        // employee's discount request was approved/rejected isn't other staff's business.
        await notificationService.SendNotificationToUsersAsync(
            [request.RequestedByUserId],
            title,
            message,
            notification.Approved ? NotificationType.Success : NotificationType.Warning);
    }
}
