using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities;

// A per-user exception to whatever their role(s) grant by default. Deny always beats Allow,
// and both always beat the role default - see Infrastructure.Implementations.PermissionResolver
// for the actual resolution order. Only one row may exist per (UserId, Permission) - enforced
// by a unique index in UserPermissionOverrideConfiguration.
public class UserPermissionOverride : ITenantScoped, IAuditableEntity
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string Permission { get; set; } = string.Empty;
    public PermissionEffect Effect { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public AppUser User { get; set; } = null!;
}
