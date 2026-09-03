using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Helpers;

namespace SportAcademy.Application.EventHandlers;

public sealed class PaymentVoidedHandler(INotificationService notificationService)
    : INotificationHandler<PaymentVoidedEvent>
{
    public async Task Handle(PaymentVoidedEvent notification, CancellationToken cancellationToken)
    {
        await notificationService.SendNotificationToGroupsAsync(
            [NotificationGroupNames.Admins, NotificationGroupNames.Owners],
            "Payment Voided",
            $"Payment {notification.PaymentNumber} was voided by {notification.ActorName}",
            NotificationType.Warning);
    }
}
