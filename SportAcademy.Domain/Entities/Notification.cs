using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class Notification
    {
        public int Id { get; private set; }
        public string Message { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; }
        public string? GroupName { get; private set; }
        public string? Title { get; private set; }
        public NotificationType? Type { get; private set; }
        public string? ActionUrl { get; private set; }

        public virtual ICollection<NotificationRecipient> Recipients { get; set; } = [];

        private Notification() { }

        private Notification(string message, string? title, NotificationType? type,
            string? groupName, string? actionUrl)
        {
            Message = message;
            Title = title;
            Type = type;
            GroupName = groupName;
            ActionUrl = actionUrl;
            CreatedAt = DateTime.UtcNow;
        }

        public static Notification Create(string message, string? title = null,
            NotificationType? type = null, string? groupName = null, string? actionUrl = null)
            => new(message, title, type, groupName, actionUrl);
    }
}
