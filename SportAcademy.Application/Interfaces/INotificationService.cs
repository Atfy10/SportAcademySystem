using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string title, string message,
            NotificationType type = NotificationType.System, string? actionUrl = null);
        Task SendNotificationToGroupAsync(string groupName, string title, string message,
            NotificationType type = NotificationType.System);
        Task BroadcastNotificationAsync(string title, string message,
            NotificationType type = NotificationType.System);

        /// Sends one notification to an explicit set of users (e.g. "every staff member plus
        /// the Owner") - for an announcement whose audience isn't one of the named SignalR
        /// groups (NotificationGroupNames.Admins/General).
        Task SendNotificationToUsersAsync(IEnumerable<Guid> userIds, string title, string message,
            NotificationType type = NotificationType.System);

        /// Pushes to every other connection the user has open (other tabs/devices) so a
        /// mark-as-read action taken in one place is reflected everywhere immediately.
        Task NotifyNotificationReadAsync(string userId, int notificationId);
        Task NotifyAllNotificationsReadAsync(string userId);
    }
}
