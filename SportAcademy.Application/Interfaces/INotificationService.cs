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
    }
}
