using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Helpers;

namespace SportAcademy.Application.EventHandlers;

public sealed class SubscriptionDiscountRequestCreatedHandler(
    INotificationService notificationService,
    IUserRepository userRepository,
    ISubscriptionDiscountRequestRepository requestRepository)
    : INotificationHandler<SubscriptionDiscountRequestCreatedEvent>
{
    public async Task Handle(SubscriptionDiscountRequestCreatedEvent notification, CancellationToken cancellationToken)
    {
        var request = await requestRepository.GetByIdWithIncludesAsync(notification.RequestId, cancellationToken);
        if (request is null) return;

        var actorName = await userRepository.GetDisplayNameAsync(request.RequestedByUserId, cancellationToken);
        var traineeName = request.Trainee is not null ? $"{request.Trainee.FirstName} {request.Trainee.LastName}" : "a trainee";

        // Only whoever can actually act on it - Owner/Admin (implicitly, via their wildcard
        // permission set) and Accountant (granted DiscountCode.Approve explicitly) hold
        // discountcode.approve. Employee (the usual requester) is deliberately excluded.
        await notificationService.SendNotificationToGroupsAsync(
            [NotificationGroupNames.Admins, NotificationGroupNames.Owners, NotificationGroupNames.Accountants],
            "New Discount Request",
            $"Discount code \"{request.DiscountCode}\" requested for {traineeName} by {actorName} needs approval",
            NotificationType.Info);
    }
}
