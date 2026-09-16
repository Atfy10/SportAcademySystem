using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly ApplicationDbContext _context;

    public TenantRepository(ApplicationDbContext context) => _context = context;

    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Set<Tenant>().FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => _context.Set<Tenant>().FirstOrDefaultAsync(t => t.Slug == slug, ct);

    public Task<Tenant?> GetDetailByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Set<Tenant>()
            .Include(t => t.Profile)
            .Include(t => t.Settings)
            .Include(t => t.Subscription).ThenInclude(s => s.Plan)
            .Include(t => t.Features).ThenInclude(f => f.Feature)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<(List<Tenant> Items, int TotalCount)> GetPagedAsync(
        int skip, int take, string? status, string? search, CancellationToken ct = default)
    {
        // The System tenant is the platform's own bookkeeping record (SuperAdmin's TenantId
        // claim), not a customer academy - it must never appear in the Platform's tenant list.
        var query = _context.Set<Tenant>().Where(t => t.Code != Tenant.SystemTenantCode);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TenantStatus>(status, ignoreCase: true, out var statusEnum))
            query = query.Where(t => t.Status == statusEnum);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(t =>
                t.Name.ToLower().Contains(term) ||
                t.DisplayName.ToLower().Contains(term) ||
                t.Code.ToLower().Contains(term) ||
                t.Email.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Include(t => t.Subscription)
                .ThenInclude(s => s.Plan)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public Task<int> GetCountAsync(CancellationToken ct = default)
        => _context.Set<Tenant>().CountAsync(t => t.Code != Tenant.SystemTenantCode, ct);

    public async Task<Dictionary<string, int>> GetStatusCountsAsync(CancellationToken ct = default)
    {
        var counts = await _context.Set<Tenant>()
            .Where(t => t.Code != Tenant.SystemTenantCode)
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync(ct);

        return counts.ToDictionary(c => c.Status, c => c.Count);
    }

    public Task<int> GetTotalUsersAsync(CancellationToken ct = default)
        => _context.Set<AppUser>().IgnoreQueryFilters().CountAsync(ct);

    public Task<int> GetTotalBranchesAsync(CancellationToken ct = default)
        => _context.Set<Branch>().IgnoreQueryFilters().CountAsync(ct);

    public Task<int> GetUserCountByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => _context.Set<AppUser>().CountAsync(u => u.TenantId == tenantId, ct);

    public Task<int> GetBranchCountByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => _context.Set<Branch>().CountAsync(b => b.TenantId == tenantId, ct);

    public Task<int> GetSportCountByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => _context.Set<Sport>().CountAsync(s => s.TenantId == tenantId, ct);

    public async Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = _context.Set<Tenant>().Where(t => t.Slug == slug);
        if (excludeId.HasValue)
            query = query.Where(t => t.Id != excludeId.Value);
        return !await query.AnyAsync(ct);
    }

    public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = _context.Set<Tenant>().Where(t => t.Code == code);
        if (excludeId.HasValue)
            query = query.Where(t => t.Id != excludeId.Value);
        return !await query.AnyAsync(ct);
    }

    public async Task AddAsync(Tenant tenant, CancellationToken ct = default)
        => await _context.Set<Tenant>().AddAsync(tenant, ct);

    public Task<TenantFeature?> GetTenantFeatureAsync(Guid tenantId, Guid featureId, CancellationToken ct = default)
        => _context.Set<TenantFeature>()
            .FirstOrDefaultAsync(tf => tf.TenantId == tenantId && tf.FeatureId == featureId, ct);

    public async Task<List<TenantFeature>> GetTenantFeaturesAsync(Guid tenantId, CancellationToken ct = default)
        => await _context.Set<TenantFeature>()
            .Include(tf => tf.Feature)
            .Where(tf => tf.TenantId == tenantId)
            .ToListAsync(ct);

    public Task<bool> IsFeatureEnabledAsync(Guid tenantId, string featureName, CancellationToken ct = default)
        => _context.Set<TenantFeature>()
            .AnyAsync(tf => tf.TenantId == tenantId && tf.Feature.Name == featureName && tf.IsEnabled, ct);

    public async Task AddTenantFeatureAsync(TenantFeature feature, CancellationToken ct = default)
        => await _context.Set<TenantFeature>().AddAsync(feature, ct);

    public Task<List<Feature>> GetAllFeaturesAsync(CancellationToken ct = default)
        => _context.Set<Feature>().ToListAsync(ct);

    public async Task<TenantSettings?> GetSettingsAsync(Guid tenantId, CancellationToken ct = default)
        => await _context.Set<TenantSettings>().FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);

    public void UpdateSettings(TenantSettings settings)
        => _context.Set<TenantSettings>().Update(settings);

    public async Task<TenantProfile?> GetProfileAsync(Guid tenantId, CancellationToken ct = default)
        => await _context.Set<TenantProfile>().FirstOrDefaultAsync(p => p.TenantId == tenantId, ct);

    public void UpdateProfile(TenantProfile profile)
        => _context.Set<TenantProfile>().Update(profile);

    public async Task<List<Guid>> GetPlanFeaturesAsync(int planId, CancellationToken ct = default)
        => await _context.SubscriptionPlanFeatures
            .Where(spf => spf.SubscriptionPlanId == planId)
            .Select(spf => spf.FeatureId)
            .ToListAsync(ct);

    public async Task ReplacePlanFeaturesAsync(int planId, List<Guid> featureIds, CancellationToken ct = default)
    {
        var existing = await _context.SubscriptionPlanFeatures
            .Where(spf => spf.SubscriptionPlanId == planId)
            .ToListAsync(ct);
        _context.SubscriptionPlanFeatures.RemoveRange(existing);

        _context.SubscriptionPlanFeatures.AddRange(featureIds.Select(featureId => new SubscriptionPlanFeature
        {
            SubscriptionPlanId = planId,
            FeatureId = featureId
        }));
    }

    public async Task<List<Guid>> GetTenantIdsSubscribedToPlanAsync(int planId, CancellationToken ct = default)
        => await _context.TenantSubscriptions
            .Where(s => s.SubscriptionPlanId == planId)
            .Select(s => s.TenantId)
            .ToListAsync(ct);

    public async Task BulkUpdateFeaturesAsync(Guid tenantId, Dictionary<Guid, bool> featureStates, string enabledBy, CancellationToken ct = default)
    {
        foreach (var (featureId, isEnabled) in featureStates)
        {
            var existing = await _context.Set<TenantFeature>()
                .FirstOrDefaultAsync(tf => tf.TenantId == tenantId && tf.FeatureId == featureId, ct);

            if (existing is not null)
            {
                // Super-Admin-locked features are skipped, not failed: a bulk save covers many
                // features at once and one locked entry shouldn't block the rest from saving.
                if (existing.LockedBySuperAdmin)
                    continue;

                if (existing.IsEnabled != isEnabled)
                {
                    existing.IsEnabled = isEnabled;
                    existing.EnabledAt = DateTime.UtcNow;
                    existing.EnabledBy = enabledBy;
                }
            }
            else if (isEnabled)
            {
                await _context.Set<TenantFeature>().AddAsync(new TenantFeature
                {
                    TenantId = tenantId,
                    FeatureId = featureId,
                    IsEnabled = true,
                    EnabledAt = DateTime.UtcNow,
                    EnabledBy = enabledBy
                }, ct);
            }
        }
    }

    // ---- Plan/tenant limits ----

    public Task<int?> GetCurrentPlanIdAsync(Guid tenantId, CancellationToken ct = default)
        => _context.TenantSubscriptions
            .Where(s => s.TenantId == tenantId)
            .Select(s => (int?)s.SubscriptionPlanId)
            .FirstOrDefaultAsync(ct);

    public Task<List<PlanLimit>> GetPlanLimitsAsync(int planId, CancellationToken ct = default)
        => _context.PlanLimits
            .Where(pl => pl.SubscriptionPlanId == planId)
            .ToListAsync(ct);

    public async Task ReplacePlanLimitsAsync(int planId, Dictionary<string, int?> limits, CancellationToken ct = default)
    {
        var existing = await _context.PlanLimits
            .Where(pl => pl.SubscriptionPlanId == planId)
            .ToListAsync(ct);
        _context.PlanLimits.RemoveRange(existing);

        _context.PlanLimits.AddRange(limits.Select(kv => new PlanLimit
        {
            SubscriptionPlanId = planId,
            ResourceKey = kv.Key,
            MaxCount = kv.Value
        }));
    }

    public Task<TenantLimitOverride?> GetTenantLimitOverrideAsync(Guid tenantId, string resourceKey, CancellationToken ct = default)
        => _context.TenantLimitOverrides
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.ResourceKey == resourceKey, ct);

    public Task<List<TenantLimitOverride>> GetTenantLimitOverridesAsync(Guid tenantId, CancellationToken ct = default)
        => _context.TenantLimitOverrides
            .Where(o => o.TenantId == tenantId)
            .ToListAsync(ct);

    public async Task SetTenantLimitOverrideAsync(TenantLimitOverride @override, CancellationToken ct = default)
    {
        var existing = await _context.TenantLimitOverrides
            .FirstOrDefaultAsync(o => o.TenantId == @override.TenantId && o.ResourceKey == @override.ResourceKey, ct);

        if (existing is null)
        {
            await _context.TenantLimitOverrides.AddAsync(@override, ct);
            return;
        }

        existing.MaxCount = @override.MaxCount;
        existing.SetBy = @override.SetBy;
        existing.SetAt = @override.SetAt;
        existing.Reason = @override.Reason;
    }

    public async Task RemoveTenantLimitOverrideAsync(Guid tenantId, string resourceKey, CancellationToken ct = default)
    {
        var existing = await _context.TenantLimitOverrides
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.ResourceKey == resourceKey, ct);

        if (existing is not null)
            _context.TenantLimitOverrides.Remove(existing);
    }

    // "Used" here means "currently consuming a seat" - a materially different question from the
    // raw, all-time GetBranchCountByTenantAsync/GetUserCountByTenantAsync/
    // GetSportCountByTenantAsync above, which back the platform dashboard and count every row
    // regardless of IsActive/IsBanned. IsDeleted is not checked explicitly for AppUser/Trainee -
    // both are ISoftDeletable, and the global soft-delete query filter already excludes deleted
    // rows from every unfiltered query, this one included.
    //
    // IgnoreQueryFilters() is required, not optional, on every query below - Branch/Sport/
    // AppUser/Trainee/Invitation are all ITenantScoped, and every caller that actually matters
    // for this method (ChangeTenantPlanCommandHandler, UpdatePlanLimitsCommandHandler,
    // SetTenantLimitOverrideCommandHandler, the reopen/bypass commands) runs under the SuperAdmin's
    // OWN ambient tenant (the platform's System tenant), not the tenantId parameter. Without this,
    // the global filter silently ANDs in "AND TenantId == SystemTenantId" on top of the explicit
    // "AND TenantId == tenantId" below, matching zero rows for any real customer tenant - usage
    // was permanently computed as 0 and PendingLimitSelection could never trigger from the only
    // path that ever triggers it. The explicit "TenantId == tenantId" filter already provides the
    // real tenant boundary, so this is exactly as safe as
    // GetUserIdsByTenantIgnoringTenantAsync's identical pattern above.
    public async Task<Dictionary<string, int>> GetResourceUsageAsync(Guid tenantId, CancellationToken ct = default)
    {
        var branches = await _context.Set<Branch>()
            .IgnoreQueryFilters()
            .CountAsync(b => b.TenantId == tenantId && b.IsActive, ct);

        var sports = await _context.Set<Sport>()
            .IgnoreQueryFilters()
            .CountAsync(s => s.TenantId == tenantId && s.IsActive, ct);

        var trainees = await _context.Set<Trainee>()
            .IgnoreQueryFilters()
            .CountAsync(t => t.TenantId == tenantId && !t.IsDeleted, ct);

        // A pending, non-expired invitation reserves the seat it would fill on acceptance -
        // otherwise an Owner could send far more invitations than their plan allows (each one
        // individually under the cap at send time) and the cap would be bypassed wholesale the
        // moment they're all accepted. AcceptInvitationCommandHandler still re-checks
        // independently at acceptance time, since headroom that existed when the invite was
        // sent can be gone by the time it's used.
        var now = DateTime.UtcNow;
        var pendingInvitations = await _context.Set<Invitation>()
            .IgnoreQueryFilters()
            .CountAsync(i => i.TenantId == tenantId && i.Status == InvitationStatus.Pending && i.ExpiresAt > now, ct);

        var users = await _context.Set<AppUser>()
            .IgnoreQueryFilters()
            .CountAsync(u => u.TenantId == tenantId && !u.IsBanned && !u.IsDeleted, ct);

        return new Dictionary<string, int>
        {
            [LimitedResources.Branches] = branches,
            [LimitedResources.Users] = users + pendingInvitations,
            [LimitedResources.Sports] = sports,
            [LimitedResources.Trainees] = trainees,
        };
    }

    // ---- Downgrade-reconciliation tasks ----

    public Task<TenantLimitReconciliation?> GetOpenReconciliationAsync(Guid tenantId, CancellationToken ct = default)
        => _context.TenantLimitReconciliations
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.CompletedAt == null, ct);

    public async Task AddReconciliationAsync(TenantLimitReconciliation reconciliation, CancellationToken ct = default)
        => await _context.TenantLimitReconciliations.AddAsync(reconciliation, ct);

    public Task<List<TenantLimitReconciliation>> GetOpenReconciliationsPastDeadlineAsync(DateTime cutoffUtc, CancellationToken ct = default)
        => _context.TenantLimitReconciliations
            .Where(r => r.CompletedAt == null && r.DeadlineAt < cutoffUtc)
            .ToListAsync(ct);

    public Task<List<TenantLimitReconciliation>> GetAllOpenReconciliationsAsync(CancellationToken ct = default)
        => _context.TenantLimitReconciliations
            .Include(r => r.Tenant)
            .Where(r => r.CompletedAt == null)
            .OrderBy(r => r.DeadlineAt)
            .ToListAsync(ct);
}
