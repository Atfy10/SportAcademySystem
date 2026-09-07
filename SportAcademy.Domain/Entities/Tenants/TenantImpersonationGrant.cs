namespace SportAcademy.Domain.Entities.Tenants;

// A time-boxed, read-only cross-tenant access grant a SuperAdmin opens against a specific
// tenant (Phase 3 of the platform hardening work - see the artifact plan). Deliberately not
// ITenantScoped, same reasoning as TenantAuditEvent: the platform operator's own tenant is
// System, not the one being accessed.
//
// A grant does not itself carry permissions - see StartImpersonationCommandHandler for how the
// short-lived access token it backs is built, and ImpersonationGuardMiddleware for how every
// request under it is confirmed still active and restricted to reads.
public class TenantImpersonationGrant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid GrantedByUserId { get; set; }

    /// <summary>Required - why this session was opened, so the audit trail (and the tenant's
    /// own Owner, if this is ever surfaced to them) can see it was for a stated purpose.</summary>
    public string Reason { get; set; } = null!;

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Hard cap on how long the access token this grant backs remains valid,
    /// regardless of activity - see StartImpersonationCommandHandler.MaxDurationMinutes.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Null while the grant is still open. Set either by EndImpersonationCommand (a
    /// deliberate "End session") or lazily by ImpersonationGuardMiddleware the first time a
    /// request arrives after ExpiresAt has passed.</summary>
    public DateTime? EndedAt { get; set; }

    /// <summary>"Manual" or "Expired" once EndedAt is set; null while open.</summary>
    public string? EndedReason { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
