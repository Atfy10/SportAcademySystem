using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public required string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? GroupName { get; set; }
        public string? Title { get; set; }
        public NotificationType? Type { get; set; }
        public string? ActionUrl { get; set; }

        public virtual ICollection<NotificationRecipient> Recipients { get; set; } = [];
    }
}
