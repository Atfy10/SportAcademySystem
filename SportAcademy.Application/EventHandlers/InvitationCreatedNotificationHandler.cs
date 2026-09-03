using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Events;
using SportAcademy.Domain.Helpers;

namespace SportAcademy.Application.EventHandlers;

/// Separate from InvitationCreatedHandler (which sends the invite email to the invitee) - this
/// is the in-app "an invitation was sent" notification for existing staff, not the invitee
/// (who has no account/notifications until they accept).
public sealed class InvitationCreatedNotificationHandler(INotificationService notificationService)
    : INotificationHandler<InvitationCreatedEvent>
{
    public async Task Handle(InvitationCreatedEvent notification, CancellationToken cancellationToken)
    {
        await notificationService.SendNotificationToGroupsAsync(
            [NotificationGroupNames.Admins, NotificationGroupNames.Owners],
            "Invitation Sent",
            $"An invitation was sent to {notification.Email} by {notification.ActorName}",
            NotificationType.Info);
    }
}
