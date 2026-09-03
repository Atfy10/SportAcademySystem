using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string title, string message,
            NotificationType type = NotificationType.System, string? actionUrl = null);
        Task SendNotificationToGroupAsync(string groupName, string title, string message,
            NotificationType type = NotificationType.System);

        /// Sends one notification to the union of every named group's current members (e.g.
        /// Admins + Owners + Employees) plus any explicit extra user ids (e.g. an event's
        /// originator, in case they aren't already covered by one of the groups) - membership
        /// for each group is resolved live from role/employment data, never a connection-time
        /// cache, so it reaches everyone currently qualifying regardless of SignalR connection
        /// history.
        Task SendNotificationToGroupsAsync(IEnumerable<string> groupNames, string title, string message,
            NotificationType type = NotificationType.System, IEnumerable<Guid>? extraUserIds = null);

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
