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
