namespace SportAcademy.Domain.Entities.Tenants;

// Platform-level record of a Super Admin action against a tenant (create, archive,
// status/plan change, feature toggle, subscription extend/expire/trial). Deliberately not
// ITenantScoped: it's written and read by the platform operator across every tenant, not
// filtered to "the current tenant" the way business data is.
public class TenantAuditEvent
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public string EventType { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string PerformedBy { get; set; } = "SuperAdmin";
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
}
