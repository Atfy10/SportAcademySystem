using SportAcademy.Application.Common.Limits;
using SportAcademy.Domain.Contract;

namespace SportAcademy.Infrastructure.Implementations;

public class EffectiveLimitService : IEffectiveLimitService
{
    private readonly ITenantRepository _tenantRepository;

    public EffectiveLimitService(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<EffectiveLimit> GetAsync(Guid tenantId, string resourceKey, CancellationToken ct = default)
    {
        var all = await GetAllAsync(tenantId, ct);
        return all.First(l => l.ResourceKey == resourceKey);
    }

    public async Task<IReadOnlyList<EffectiveLimit>> GetAllAsync(Guid tenantId, CancellationToken ct = default)
    {
        var usage = await _tenantRepository.GetResourceUsageAsync(tenantId, ct);
        var overrides = await _tenantRepository.GetTenantLimitOverridesAsync(tenantId, ct);
        var overrideByKey = overrides.ToDictionary(o => o.ResourceKey);

        var planId = await _tenantRepository.GetCurrentPlanIdAsync(tenantId, ct);
        var planLimitByKey = planId is { } id
            ? (await _tenantRepository.GetPlanLimitsAsync(id, ct)).ToDictionary(pl => pl.ResourceKey)
            : new Dictionary<string, Domain.Entities.Tenants.PlanLimit>();

        var results = new List<EffectiveLimit>(LimitedResources.All.Count);

        foreach (var key in LimitedResources.All)
        {
            var used = usage.GetValueOrDefault(key, 0);

            // Resolution order: override always wins (even upward - a SuperAdmin override
            // can raise a limit above what the plan grants, not just cap it lower), then the
            // plan, then unlimited. Same "a forced decision overrides plan membership
            // entirely" rule TenantFeature.LockedBySuperAdmin already applies to features.
            if (overrideByKey.TryGetValue(key, out var over))
            {
                results.Add(new EffectiveLimit(key, over.MaxCount, LimitSource.Override, used, over.Reason));
            }
            else if (planLimitByKey.TryGetValue(key, out var planLimit))
            {
                results.Add(new EffectiveLimit(key, planLimit.MaxCount, LimitSource.Plan, used, null));
            }
            else
            {
                // No row for this resource on this plan at all - unlimited, not zero. See
                // PlanLimit's own comment: this is what lets a newly-introduced limited
                // resource not retroactively cap every pre-existing plan at zero.
                results.Add(new EffectiveLimit(key, null, LimitSource.Unlimited, used, null));
            }
        }

        return results;
    }
}
