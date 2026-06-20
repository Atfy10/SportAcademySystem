namespace SportAcademy.Domain.Entities
{
    public class NotificationRecipient
    {
        public bool IsRead { get; private set; }
        public int NotificationId { get; private set; }
        public string UserId { get; private set; } = null!;

        public virtual AppUser User { get; set; } = null!;
        public virtual Notification Notification { get; set; } = null!;

        private NotificationRecipient() { }

        private NotificationRecipient(int notificationId, string userId)
        {
            NotificationId = notificationId;
            UserId = userId;
            IsRead = false;
        }

        public static NotificationRecipient Create(int notificationId, string userId)
            => new(notificationId, userId);

        public static NotificationRecipient CreateWithNotification(Notification notification, string userId)
        {
            var entity = new NotificationRecipient();
            entity.Notification = notification;
            entity.UserId = userId;
            return entity;
        }

        public void MarkAsRead()
        {
            IsRead = true;
        }
    }
}
