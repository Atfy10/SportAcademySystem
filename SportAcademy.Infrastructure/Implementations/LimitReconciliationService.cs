using System.Text.Json;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Infrastructure.Implementations;

public class LimitReconciliationService : ILimitReconciliationService
{
    /// <summary>How long a tenant gets to complete the forced selection before being suspended -
    /// see LimitReconciliationDeadlineService, which mirrors EnrollmentLapseService.GracePeriodDays'
    /// exact shape for this constant.</summary>
    public const int GraceDays = 7;

    private readonly ITenantRepository _tenantRepository;
    private readonly IEffectiveLimitService _limitService;
    private readonly ITenantStatusCacheInvalidator _tenantStatusCache;
    private readonly IRealtimeService _realtimeService;

    public LimitReconciliationService(
        ITenantRepository tenantRepository,
        IEffectiveLimitService limitService,
        ITenantStatusCacheInvalidator tenantStatusCache,
        IRealtimeService realtimeService)
    {
        _tenantRepository = tenantRepository;
        _limitService = limitService;
        _tenantStatusCache = tenantStatusCache;
        _realtimeService = realtimeService;
    }

    public async Task<bool> EvaluateAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, ct);
        if (tenant is null)
            return false;

        // Already mid-reconciliation - don't clobber an in-progress selection with a fresh
        // snapshot just because another limit-affecting change landed in the meantime. The
        // existing window (and its deadline) stands until the tenant completes it or it lapses.
        if (tenant.Status == TenantStatus.PendingLimitSelection)
            return false;

        // Only an Active tenant can be pushed into this state - TenantStatusPolicy only allows
        // (Active, PendingLimitSelection). A Suspended/Archived/PendingSetup tenant has a bigger
        // problem than its seat count, and reconciling it here would reactivate it via the wrong
        // path (this transition is the only one that skips ChangeTenantStatusCommand's own
        // guard against setting PendingLimitSelection directly - see that command's validator).
        if (tenant.Status != TenantStatus.Active)
            return false;

        var limits = await _limitService.GetAllAsync(tenantId, ct);
        var overLimits = limits
            .Where(l => !l.HasHeadroom && LimitedResources.RequiresSelection(l.ResourceKey))
            .ToList();

        // Trainees over cap (or any resource that doesn't require selection) never locks the
        // tenant - see LimitedResources.RequiresSelection's own comment: grandfathered, only new
        // registration is blocked (D6 in PLAN_LIMITS_DESIGN.md).
        if (overLimits.Count == 0)
            return false;

        var planId = await _tenantRepository.GetCurrentPlanIdAsync(tenantId, ct) ?? 0;
        var requiredResources = overLimits.ToDictionary(l => l.ResourceKey, l => l.Used);
        var now = DateTime.UtcNow;

        await _tenantRepository.AddReconciliationAsync(new TenantLimitReconciliation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TriggeredByPlanId = planId,
            OpenedAt = now,
            DeadlineAt = now.AddDays(GraceDays),
            RequiredResourcesJson = JsonSerializer.Serialize(requiredResources),
        }, ct);

        tenant.Status = TenantStatus.PendingLimitSelection;

        // Stages the status flip only - the caller's own SaveChangesAsync commits this
        // atomically alongside whatever change triggered the evaluation (plan change, limit
        // edit, override change), same convention ReplacePlanFeaturesAsync/
        // BulkUpdateFeaturesAsync already use elsewhere on this repository.
        _tenantStatusCache.Invalidate(tenantId);

        // Same channel ChangeTenantStatusCommandHandler uses for Suspended/Archived - the
        // frontend's realtime handler distinguishes PendingLimitSelection from those (routes to
        // the reconciliation wizard, does not force a logout - see RealtimeContext.tsx).
        await _realtimeService.NotifyTenantStatusChangedAsync(tenantId, TenantStatus.PendingLimitSelection.ToString());

        return true;
    }

    public async Task ReopenAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, ct)
            ?? throw new InvalidOperationException($"ReopenAsync called for a tenant that doesn't exist: {tenantId}.");

        // A fresh snapshot, not the stale one from whenever this tenant first lapsed - a
        // SuperAdmin reopening the window days or weeks later deserves current numbers.
        var limits = await _limitService.GetAllAsync(tenantId, ct);
        var overLimits = limits.Where(l => !l.HasHeadroom && LimitedResources.RequiresSelection(l.ResourceKey)).ToList();
        var requiredResources = overLimits.ToDictionary(l => l.ResourceKey, l => l.Used);

        var planId = await _tenantRepository.GetCurrentPlanIdAsync(tenantId, ct) ?? 0;
        var now = DateTime.UtcNow;

        await _tenantRepository.AddReconciliationAsync(new TenantLimitReconciliation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TriggeredByPlanId = planId,
            OpenedAt = now,
            DeadlineAt = now.AddDays(GraceDays),
            RequiredResourcesJson = JsonSerializer.Serialize(requiredResources),
        }, ct);

        tenant.Status = TenantStatus.PendingLimitSelection;
        _tenantStatusCache.Invalidate(tenantId);
        await _realtimeService.NotifyTenantStatusChangedAsync(tenantId, TenantStatus.PendingLimitSelection.ToString());
    }
}
