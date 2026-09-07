using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities.Tenants;

// Platform-level record of a Super Admin action against a tenant (create, archive,
// status/plan change, feature toggle, subscription extend/expire/trial, owner ban/reset-link).
// Deliberately not ITenantScoped: it's written and read by the platform operator across every
// tenant, not filtered to "the current tenant" the way business data is.
//
// Written exclusively by PlatformAuditBehavior (for every IAuditableCommand, in the same
// transaction as the handler it audits - see that class) and by PermissionAuthorizationHandler
// (for a denied platform policy check, which never reaches a handler to audit). Never updated or
// deleted after insert - see AuditImmutabilityInterceptor.
public class TenantAuditEvent
{
    public int Id { get; set; }

    /// <summary>Null for a platform-wide event with no single target tenant: a denied
    /// authorization attempt at a route like GET /api/platform/tenants (no tenant in scope to
    /// attribute it to), or a CreateTenantCommand that failed before a tenant ever existed
    /// (a duplicate slug/code, say).</summary>
    public Guid? TenantId { get; set; }
    public string EventType { get; set; } = null!;
    public string Description { get; set; } = null!;
    public AuditOutcome Outcome { get; set; }

    /// <summary>
    /// Free-text explanation for a Failed/Denied outcome, distinct from Description (which
    /// states what was attempted). Null on a Succeeded event.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>The command payload as submitted, serialized as JSON. Null only for the
    /// PermissionAuthorizationHandler-authored Denied events, which have no command to
    /// serialize - authorization fails before MediatR ever resolves one.</summary>
    public string? AfterJson { get; set; }

    /// <summary>
    /// Reserved for a future per-entity before/after snapshot. Deliberately left null rather
    /// than populated with a shallow, potentially misleading diff - see PlatformAuditBehavior's
    /// class comment for why this isn't populated yet.
    /// </summary>
    public string? BeforeJson { get; set; }

    /// <summary>The authenticated actor. Never null - every write path that creates one of
    /// these runs after authentication has already succeeded.</summary>
    public Guid PerformedByUserId { get; set; }

    /// <summary>Denormalized display name, purely for rendering the log - PerformedByUserId is
    /// the actual identity. A username change after the fact does not rewrite history.</summary>
    public string PerformedBy { get; set; } = null!;

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    /// <summary>Ties this event back to the same request's application logs - see
    /// ExceptionHandlingBehavior.CurrentTraceId() for the identical convention.</summary>
    public string? CorrelationId { get; set; }

    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

    public Tenant? Tenant { get; set; }
}
