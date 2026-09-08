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

    // The invitation link itself only proves the recipient has whatever URL was sent/copied/
    // forwarded - it doesn't prove they're the one actually reading Email right now. This closes
    // that gap: a fresh 6-digit code must be requested and correctly entered before Accept() is
    // allowed to run (see AcceptInvitationCommandHandler). Hashed the same way TokenHash is -
    // never stored in plain text.
    public bool IsEmailVerified { get; set; }
    public string? VerificationCodeHash { get; set; }
    public DateTime? VerificationCodeExpiresAt { get; set; }
    public int VerificationCodeAttempts { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public void Accept()
    {
        Status = InvitationStatus.Accepted;
        UsedAt = DateTime.UtcNow;
    }

    // Called once per SendInvitationVerificationCodeCommand - always replaces whatever code
    // (if any) was issued before, so only the most recently sent code is ever valid, and resets
    // the attempt counter so an earlier round of wrong guesses can't carry over and lock out a
    // legitimately fresh code.
    public void SetVerificationCode(string codeHash, DateTime expiresAt)
    {
        VerificationCodeHash = codeHash;
        VerificationCodeExpiresAt = expiresAt;
        VerificationCodeAttempts = 0;
    }

    public void MarkEmailVerified()
    {
        IsEmailVerified = true;
        VerificationCodeHash = null;
        VerificationCodeExpiresAt = null;
        VerificationCodeAttempts = 0;
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
