using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Domain.Entities;

// One row per (UserId, BranchId) the user is allowed to see - only meaningful for users whose
// role is branch-restricted (currently just "Employee"; see IBranchAccessProvider). A user with
// zero rows here and a branch-restricted role sees no branch-scoped data at all, not everything -
// branches must be explicitly granted, same "deny by default" posture as tenant isolation.
public class UserBranchAccess : ITenantScoped, IAuditableEntity
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public int BranchId { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public AppUser User { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
}
