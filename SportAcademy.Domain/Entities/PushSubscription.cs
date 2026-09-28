namespace SportAcademy.Domain.Entities
{
    /// <summary>
    /// One browser/device Web Push subscription (RFC 8030), created client-side via
    /// PushManager.subscribe() and registered through PushSubscriptionsController. Not
    /// ITenantScoped - keyed off AppUser.Id directly, same tier as UserNotificationPreference: a
    /// user's own devices, not a tenant-owned resource. A user can have several rows (one per
    /// browser/device); PushChannelSender fans out to all of them and prunes any that the push
    /// service reports as gone (404/410).
    /// </summary>
    public class PushSubscription
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public required string Endpoint { get; set; }
        public required string P256dh { get; set; }
        public required string Auth { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual AppUser User { get; set; } = null!;
    }
}
