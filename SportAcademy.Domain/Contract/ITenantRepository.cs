using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Contract;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Tenant?> GetDetailByIdAsync(Guid id, CancellationToken ct = default);
    Task<(List<Tenant> Items, int TotalCount)> GetPagedAsync(
        int skip, int take, string? status, string? search, CancellationToken ct = default);
    Task<int> GetCountAsync(CancellationToken ct = default);
    Task<Dictionary<string, int>> GetStatusCountsAsync(CancellationToken ct = default);
    Task<int> GetTotalUsersAsync(CancellationToken ct = default);
    Task<int> GetTotalBranchesAsync(CancellationToken ct = default);
    Task<int> GetUserCountByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<int> GetBranchCountByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<int> GetSportCountByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeId = null, CancellationToken ct = default);
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null, CancellationToken ct = default);
    Task AddAsync(Tenant tenant, CancellationToken ct = default);
    Task<TenantFeature?> GetTenantFeatureAsync(Guid tenantId, Guid featureId, CancellationToken ct = default);
    Task<List<TenantFeature>> GetTenantFeaturesAsync(Guid tenantId, CancellationToken ct = default);
    Task AddTenantFeatureAsync(TenantFeature feature, CancellationToken ct = default);
    Task<List<Domain.Entities.Feature>> GetAllFeaturesAsync(CancellationToken ct = default);
    Task<TenantSettings?> GetSettingsAsync(Guid tenantId, CancellationToken ct = default);
    void UpdateSettings(TenantSettings settings);
    Task<TenantProfile?> GetProfileAsync(Guid tenantId, CancellationToken ct = default);
    void UpdateProfile(TenantProfile profile);
    Task<List<Guid>> GetPlanFeaturesAsync(int planId, CancellationToken ct = default);
    // Replaces a plan's entire feature set (remove-then-add) - used when a SuperAdmin edits
    // which features a plan grants.
    Task ReplacePlanFeaturesAsync(int planId, List<Guid> featureIds, CancellationToken ct = default);
    Task<List<Guid>> GetTenantIdsSubscribedToPlanAsync(int planId, CancellationToken ct = default);
    Task BulkUpdateFeaturesAsync(Guid tenantId, Dictionary<Guid, bool> featureStates, string enabledBy, CancellationToken ct = default);
    // Single targeted check for FeatureGateBehavior - avoids loading the tenant's full feature
    // list on every gated request. Matches GetTenantFeaturesQueryHandler's semantics: no row
    // for this tenant+feature counts as disabled, not enabled-by-default.
    Task<bool> IsFeatureEnabledAsync(Guid tenantId, string featureName, CancellationToken ct = default);

    // ---- Plan/tenant limits (quantitative twin of the feature methods above) ----

    /// <summary>Null if the tenant has no active subscription row (shouldn't happen for a real
    /// tenant, but CreateTenantCommandHandler creates the Subscription in the same call that
    /// creates the Tenant, so this can't be assumed non-null before that finishes).</summary>
    Task<int?> GetCurrentPlanIdAsync(Guid tenantId, CancellationToken ct = default);
    Task<List<PlanLimit>> GetPlanLimitsAsync(int planId, CancellationToken ct = default);

    /// <summary>Replaces a plan's entire limit set (remove-then-add), mirroring
    /// ReplacePlanFeaturesAsync. A null MaxCount in the dictionary seeds an explicit "unlimited"
    /// row rather than omitting the resource - see UpdatePlanLimitsCommand.</summary>
    Task ReplacePlanLimitsAsync(int planId, Dictionary<string, int?> limits, CancellationToken ct = default);

    Task<TenantLimitOverride?> GetTenantLimitOverrideAsync(Guid tenantId, string resourceKey, CancellationToken ct = default);
    Task<List<TenantLimitOverride>> GetTenantLimitOverridesAsync(Guid tenantId, CancellationToken ct = default);
    Task SetTenantLimitOverrideAsync(TenantLimitOverride @override, CancellationToken ct = default);
    Task RemoveTenantLimitOverrideAsync(Guid tenantId, string resourceKey, CancellationToken ct = default);

    /// <summary>Current usage for every LimitedResources key, in one round trip - what a
    /// tenant is actually consuming right now, not the raw all-time counts
    /// GetBranchCountByTenantAsync/GetUserCountByTenantAsync/GetSportCountByTenantAsync return
    /// for the platform dashboard. See EffectiveLimitService for exactly what "used" means per
    /// resource (active branches, non-banned users + pending invitations, active sports,
    /// non-deleted trainees).</summary>
    Task<Dictionary<string, int>> GetResourceUsageAsync(Guid tenantId, CancellationToken ct = default);
}
