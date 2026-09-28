using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    /// <summary>
    /// The outbox row for one (recipient, channel) delivery attempt of a persisted Notification.
    /// InApp deliveries are written as Sent immediately (fired inline, never queued) for
    /// matrix/audit consistency; Email/Push/WhatsApp start Pending and are picked up by
    /// NotificationDeliveryWorker. ResolvedDestination snapshots the resolved email/phone at
    /// enqueue time so a retry stays stable against a later contact-info edit and the row is
    /// self-contained for auditing.
    /// </summary>
    public class NotificationDelivery : ITenantScoped
    {
        public long Id { get; set; }
        public int NotificationId { get; set; }
        public Guid RecipientUserId { get; set; }
        public NotificationChannel Channel { get; set; }
        public NotificationDeliveryStatus Status { get; set; } = NotificationDeliveryStatus.Pending;
        public int AttemptCount { get; set; }
        public DateTime NextAttemptAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public string? LastError { get; set; }
        public string? ResolvedDestination { get; set; }

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        public virtual Notification Notification { get; set; } = null!;
    }
}
