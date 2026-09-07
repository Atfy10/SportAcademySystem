using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities.Tenants;

public class Tenant
{
    /// <summary>
    /// Code of the platform's own bookkeeping tenant - exists only so SuperAdmin (and every
    /// platform operator) has a TenantId claim to satisfy the tenant-scoping the rest of the
    /// system requires, not a real customer academy. Never shown in the Platform's tenant
    /// list/count - see callers of this constant.
    /// </summary>
    public const string SystemTenantCode = "SYSTEM";

    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public TenantStatus Status { get; set; } = TenantStatus.PendingSetup;

    public Guid? OwnerId { get; set; }
    public AppUser? Owner { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public ICollection<Branch> Branches { get; set; } = [];
    public ICollection<TenantFeature> Features { get; set; } = [];
    public ICollection<AppUser> Users { get; set; } = [];
    public TenantProfile Profile { get; set; } = null!;
    public TenantSettings Settings { get; set; } = null!;
    public TenantSubscription Subscription { get; set; } = null!;
}
