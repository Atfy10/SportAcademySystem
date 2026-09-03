using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Helpers;

namespace SportAcademy.Application.EventHandlers;

public sealed class PaymentRecordedHandler(INotificationService notificationService)
    : INotificationHandler<PaymentRecordedEvent>
{
    public async Task Handle(PaymentRecordedEvent notification, CancellationToken cancellationToken)
    {
        await notificationService.SendNotificationToGroupsAsync(
            [NotificationGroupNames.Admins, NotificationGroupNames.Owners],
            "Payment Recorded",
            $"Payment {notification.PaymentNumber} of {notification.Amount} {notification.Currency} recorded by {notification.ActorName}",
            NotificationType.Info);
    }
}
