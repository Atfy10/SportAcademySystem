using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Mappings
{
    public static class NotificationMappings
    {
        public static Notification ToNotification(string message, string? title = null,
            NotificationType? type = null, string? groupName = null, string? actionUrl = null)
            => Notification.Create(message, title, type, groupName, actionUrl);
    }
}
