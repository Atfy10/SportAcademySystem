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

        /// Pushes to every other connection the user has open (other tabs/devices) so a
        /// mark-as-read action taken in one place is reflected everywhere immediately.
        Task NotifyNotificationReadAsync(string userId, int notificationId);
        Task NotifyAllNotificationsReadAsync(string userId);
    }
}
