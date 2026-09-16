namespace SportAcademy.Domain.Enums;

public enum TenantStatus
{
    PendingSetup = 0,
    Active,
    Inactive,
    Suspended,
    Archived,

    // Appended, not inserted - this enum is persisted as an int, so every existing value's
    // number must never shift. A downgrade (plan change, plan limit edit, or override change)
    // that puts the tenant over a limit requiring forced selection (branches/users/sports - see
    // LimitedResources.RequiresSelection; trainees are grandfathered and never cause this) lands
    // here: Owner/Admin get read-only console access for a 7-day window to choose what survives,
    // enforced by TenantStatusGuardMiddleware/TenantFileAccessGuardMiddleware. See
    // LimitReconciliationService and PLAN_LIMITS_DESIGN.md §5.
    PendingLimitSelection
}
