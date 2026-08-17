using System;

namespace SportAcademy.Domain.Entities.Tenants;

public class TenantFeature
{
    public Guid TenantId { get; set; }
    public Guid FeatureId { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime EnabledAt { get; set; }
    public string EnabledBy { get; set; } = "System";

    // Once a Super Admin explicitly sets this feature for this tenant (via the Platform
    // console), it's locked at that value: tenant self-service toggles must reject changes
    // while this is true. Set by ToggleFeatureCommandHandler; never set by tenant-facing
    // commands.
    public bool LockedBySuperAdmin { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Feature Feature { get; set; } = null!;
}
