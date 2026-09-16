namespace SportAcademy.Application.Common.Limits;

// The quantitative twin of PlanFeatureReconciler, but stateful rather than a pure function - a
// limit breach has a consequence (locking the tenant into a forced-selection window) that a
// feature breach doesn't. Called by every command that can change a tenant's effective limits:
// a plan change, a plan's own limits being edited (reconciling every tenant on that plan,
// mirroring UpdatePlanFeaturesCommandHandler), or a tenant's override being lowered/removed.
public interface ILimitReconciliationService
{
    /// <summary>
    /// Checks whether the tenant is currently over any selection-requiring limit
    /// (LimitedResources.RequiresSelection) and, if so and it isn't already mid-reconciliation,
    /// opens one and moves the tenant to PendingLimitSelection. Stages changes only - the
    /// caller's own SaveChangesAsync persists them, in the same transaction as whatever change
    /// triggered this evaluation. Returns true iff a new reconciliation was opened.
    /// </summary>
    Task<bool> EvaluateAsync(Guid tenantId, CancellationToken ct = default);

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
