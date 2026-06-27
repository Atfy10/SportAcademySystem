using System;

namespace SportAcademy.Domain.Entities.Tenants;

public class TenantFeature
{
    public Guid TenantId { get; set; }
    public Guid FeatureId { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime EnabledAt { get; set; }
    public string EnabledBy { get; set; } = "System";

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Feature Feature { get; set; } = null!;
}
