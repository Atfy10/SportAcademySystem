namespace SportAcademy.Application.Common.Limits;

// The quantitative twin of PlanFeatureReconciler, but stateful rather than a pure function - a
// limit breach has a consequence (locking the tenant into a forced-selection window) that a
// feature breach doesn't. Called by every command that can change a tenant's effective limits:
// a plan change, a plan's own limits being edited (reconciling every tenant on that plan,
// mirroring UpdatePlanFeaturesCommandHandler), or a tenant's override being lowered/removed.
public interface ILimitReconciliationService
{
    /// <summary>
    /// Re-evaluates a tenant against its current effective limits and reacts however its CURRENT
    /// status calls for - callers never need to know or branch on that status themselves, and
    /// never need to care whether whatever just changed (plan swap, plan limit edit, override
    /// change) was "up" or "down": plan tiers aren't ranked (SubscriptionPlan has no rank/tier
    /// field, only a cosmetic DisplayOrder), so the only question this method ever asks is
    /// whether the tenant is over a selection-requiring limit (LimitedResources.RequiresSelection)
    /// right now, under whatever plan/override is in effect right now. Safe to call
    /// unconditionally after any limit-affecting change, for any tenant status.
    ///
    /// - Active and now over a selection-requiring limit: opens a new reconciliation (fresh
    ///   deadline, snapshot of over-limit resources) and moves the tenant to
    ///   PendingLimitSelection. Returns Opened.
    /// - Active and not over: nothing to do. Returns NoChange.
    /// - PendingLimitSelection and no longer over anything: closes the open reconciliation
    ///   (CompletedAt/CompletedByUserId) and moves the tenant back to Active - nothing left to
    ///   hand-pick since capacity now covers everything already in use. Returns Resolved.
    /// - PendingLimitSelection and still over at least one resource: leaves the tenant locked but
    ///   refreshes the existing reconciliation's required-resources snapshot in place (same
    ///   deadline), since the exact set of over-limit resources may have shifted even though the
    ///   lock itself hasn't lifted. Returns StillPending.
    /// - Tenant not found, or in any other status (Suspended/Archived/PendingSetup): untouched.
    ///   Returns NoChange. (A tenant in one of those statuses has a bigger problem than its seat
    ///   count - this method never reactivates or newly locks it; TenantStatusPolicy only allows
    ///   the (Active, PendingLimitSelection) and (PendingLimitSelection, Active) transitions here.)
    ///
    /// Commits immediately itself (rather than staging for the caller's own SaveChangesAsync) for
    /// every branch that changes tenant status, so the cache invalidation and realtime
    /// notification that follow can't race a concurrent read against an uncommitted status
    /// change. The StillPending branch also commits immediately (the refreshed snapshot), for
    /// consistency, even though it has no cache/notify step of its own.
    /// </summary>
    Task<LimitReconciliationOutcome> ReconcileAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// SuperAdmin explicitly reopens a lapsed reconciliation window for a Suspended tenant - a
    /// fresh snapshot and a fresh 7-day deadline, not a plain reactivation (see
    /// TenantStatusPolicy's (Suspended, PendingLimitSelection) transition). Deliberately doesn't
    /// require the tenant to actually be over a limit right now - a SuperAdmin choosing to give
    /// a suspended tenant another chance is a decision the service shouldn't second-guess.
    /// Stages changes only, same convention as EvaluateAsync.
    /// </summary>
    Task ReopenAsync(Guid tenantId, CancellationToken ct = default);
}

/// <summary>Outcome of ILimitReconciliationService.ReconcileAsync - lets a caller build its
/// response message without re-deriving what happened from tenant status before/after.</summary>
public enum LimitReconciliationOutcome
{
    /// <summary>Tenant not found, not in {Active, PendingLimitSelection}, or evaluated and
    /// nothing needed to change.</summary>
    NoChange,

    /// <summary>Was Active, is now over a selection-requiring limit; a new reconciliation was
    /// opened and the tenant moved to PendingLimitSelection.</summary>
    Opened,

    /// <summary>Was PendingLimitSelection, is now within every limit; the reconciliation was
    /// closed and the tenant moved back to Active.</summary>
    Resolved,

    /// <summary>Was and remains PendingLimitSelection; the required-resources snapshot was
    /// refreshed but the tenant is still locked.</summary>
    StillPending,
}
