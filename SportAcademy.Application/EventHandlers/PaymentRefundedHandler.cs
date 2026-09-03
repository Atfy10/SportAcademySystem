using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Helpers;

namespace SportAcademy.Application.EventHandlers;

public sealed class PaymentRefundedHandler(INotificationService notificationService)
    : INotificationHandler<PaymentRefundedEvent>
{
    public async Task Handle(PaymentRefundedEvent notification, CancellationToken cancellationToken)
    {
        await notificationService.SendNotificationToGroupsAsync(
            [NotificationGroupNames.Admins, NotificationGroupNames.Owners],
            "Payment Refunded",
            $"{notification.Amount} refunded on payment {notification.PaymentNumber} by {notification.ActorName}",
            NotificationType.Warning);
    }
}
