using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    /// <summary>
    /// Per-tenant, per-event-type, per-channel routing matrix cell. Absence of a row for a given
    /// (TenantId, EventTypeId, Channel) means "use the Phase-1 default" (see
    /// GetTenantNotificationMatrixQueryHandler), not "disabled" - this is what keeps an
    /// unconfigured tenant's behavior unchanged until an Owner/Admin actually touches the
    /// settings page.
    /// </summary>
    public class TenantNotificationChannelRule : ITenantScoped
    {
        public int Id { get; set; }
        public Guid EventTypeId { get; set; }
        public NotificationChannel Channel { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        public virtual NotificationEventType EventType { get; set; } = null!;
    }
}
