using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    /// <summary>
    /// A user's own opt-out layer, one row per (UserId, Channel). Not ITenantScoped - keyed off
    /// AppUser.Id directly, same tier as NotificationRecipient. InApp is exempt: it's never
    /// modeled here and always fires regardless of what a user sets for the other channels - a
    /// user can only narrow what the tenant's TenantNotificationChannelRule matrix already
    /// allows, never widen it.
    /// </summary>
    public class UserNotificationPreference
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public NotificationChannel Channel { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual AppUser User { get; set; } = null!;
    }
}
