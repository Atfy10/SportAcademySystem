using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities;

public class Invitation : ITenantScoped, IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public InvitationPurpose Purpose { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public Guid InvitedByUserId { get; set; }
    public Guid? ReplacedByInvitationId { get; set; }

    // Only set for Purpose == StaffOnboarding - the role/permissions the invited tenant
    // staff member should receive on acceptance. OwnerSetup invitations leave these null
    // and keep their hardcoded "Owner" behavior.
    public string? Role { get; set; }
    public string? Permissions { get; set; }

    // Comma-separated Branch ids, same encoding as Permissions. Only meaningful when
    // Role == "Employee" (the only branch-restricted role - see IBranchAccessProvider);
    // CreateInvitationCommandHandler requires at least one for that role and leaves this null
    // for every other role.
    public string? BranchIds { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public void Accept()
    {
        Status = InvitationStatus.Accepted;
        UsedAt = DateTime.UtcNow;
    }

    public void Revoke()
    {
        Status = InvitationStatus.Revoked;
        RevokedAt = DateTime.UtcNow;
    }

    public void Expire()
    {
        Status = InvitationStatus.Expired;
    }
}
