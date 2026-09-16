namespace SportAcademy.Domain.Entities.Tenants;

// A SuperAdmin-set exception to what the tenant's plan would otherwise grant for one resource -
// the quantitative twin of TenantFeature.LockedBySuperAdmin. Always wins over the plan, in both
// directions (a custom limit can be higher OR lower than the plan's), and survives a later plan
// change untouched - exactly the same "a forced decision overrides plan membership entirely"
// rule TenantFeature.LockedBySuperAdmin already documents.
//
// Deliberately NOT ITenantScoped, for the same reason as TenantFeature: it carries a TenantId
// but must be readable and writable by the platform console across every tenant, with no
// ambient tenant context - implementing the interface would put it behind the global
// tenant-scoping query filter and make it unreadable by design (see
// ApplicationDbContext.OnModelCreating).
public class TenantLimitOverride
{
    public Guid TenantId { get; set; }
    public string ResourceKey { get; set; } = null!;

    // null = unlimited, same convention as PlanLimit.MaxCount.
    public int? MaxCount { get; set; }

    // The SuperAdmin's name, matching TenantFeature.EnabledBy's convention.
    public string SetBy { get; set; } = null!;
    public DateTime SetAt { get; set; }

    // Shown in the console and the audit trail - required at the command layer, not enforced
    // here, so a bulk data-fix script isn't blocked by a NOT NULL it can't easily satisfy.
    public string? Reason { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
