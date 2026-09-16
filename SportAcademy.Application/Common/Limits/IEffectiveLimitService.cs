namespace SportAcademy.Application.Common.Limits;

public enum LimitSource
{
    /// <summary>No TenantLimitOverride and no PlanLimit row for this resource - no ceiling.</summary>
    Unlimited,

    /// <summary>Ceiling comes from the tenant's current SubscriptionPlan.</summary>
    Plan,

    /// <summary>
    /// A SuperAdmin set this specific tenant's ceiling directly - always wins over the plan,
    /// in both directions, exactly as TenantFeature.LockedBySuperAdmin always wins over plan
    /// membership for features.
    /// </summary>
    Override,
}

public record EffectiveLimit(string ResourceKey, int? MaxCount, LimitSource Source, int Used, string? OverrideReason)
{
    public bool IsUnlimited => MaxCount is null;
    public bool IsOverLimit => MaxCount is { } max && Used > max;
    public bool HasHeadroom => MaxCount is not { } max || Used < max;
}

// Resolves what a tenant's ceiling actually is for one resource, and how much of it is
// currently used. Deliberately NOT cached (contrast ITenantStatusCache/PermissionResolver):
// this backs write-path enforcement (LimitGateBehavior), where a stale usage count could either
// wrongly block a legitimate create or wrongly let an over-cap one through, and both are worse
// than the extra query. Resolution order per resource:
//
//   1. TenantLimitOverride row exists       -> use it            (Source = Override)
//   2. PlanLimit row exists                 -> use it            (Source = Plan)
//   3. Neither, or MaxCount is null         -> unlimited         (Source = Unlimited)
public interface IEffectiveLimitService
{
    Task<EffectiveLimit> GetAsync(Guid tenantId, string resourceKey, CancellationToken ct = default);

    /// <summary>All of LimitedResources.All for this tenant, in one round trip - what the
    /// console's usage-meters view and the downgrade evaluator both need.</summary>
    Task<IReadOnlyList<EffectiveLimit>> GetAllAsync(Guid tenantId, CancellationToken ct = default);
}
