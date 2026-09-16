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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContextService _userContext;

    public LimitReconciliationService(
        ITenantRepository tenantRepository,
        IEffectiveLimitService limitService,
        ITenantStatusCacheInvalidator tenantStatusCache,
        IRealtimeService realtimeService,
        IUnitOfWork unitOfWork,
        IUserContextService userContext)
    {
        _tenantRepository = tenantRepository;
        _limitService = limitService;
        _tenantStatusCache = tenantStatusCache;
        _realtimeService = realtimeService;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<LimitReconciliationOutcome> ReconcileAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, ct);
        if (tenant is null)
            return LimitReconciliationOutcome.NoChange;

        // Already mid-reconciliation - re-check it against the current numbers instead of
        // evaluating it as if it were still Active. Plan tiers aren't ranked (no assumption of
        // "up" or "down" anywhere here), so a tenant already locked is handled the exact same way
        // whether whatever just changed happens to raise or lower its effective limits.
        if (tenant.Status == TenantStatus.PendingLimitSelection)
            return await ReconcilePendingAsync(tenant, ct);

        // Only an Active tenant can be newly pushed into this state - TenantStatusPolicy only
        // allows (Active, PendingLimitSelection). A Suspended/Archived/PendingSetup tenant has a
        // bigger problem than its seat count, and reconciling it here would reactivate it via the
        // wrong path (this transition is the only one that skips ChangeTenantStatusCommand's own
        // guard against setting PendingLimitSelection directly - see that command's validator).
        if (tenant.Status != TenantStatus.Active)
            return LimitReconciliationOutcome.NoChange;

        var overLimits = await GetOverLimitsAsync(tenantId, ct);

        // Trainees over cap (or any resource that doesn't require selection) never locks the
        // tenant - see LimitedResources.RequiresSelection's own comment: grandfathered, only new
        // registration is blocked (D6 in PLAN_LIMITS_DESIGN.md).
        if (overLimits.Count == 0)
            return LimitReconciliationOutcome.NoChange;

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

        // Commit now, before invalidating the cache or notifying, rather than leaving this
        // staged for the caller's own later SaveChangesAsync. Invalidating first and committing
        // later leaves a window where a concurrent request (e.g. another open tab's permission
        // poll) can miss the now-empty cache, read the still-Active row because this transaction
        // hasn't landed yet, and re-cache that stale status for the full sliding TTL - which is
        // exactly what made the tenant "look active" after a downgrade in testing. Saving here
        // also commits whatever the caller staged before calling ReconcileAsync (plan change,
        // limit edit, override change) - that's fine, it was already fully validated upstream,
        // and it just means the caller's own subsequent SaveChangesAsync becomes a no-op.
        await _unitOfWork.SaveChangesAsync(ct);

        _tenantStatusCache.Invalidate(tenantId);

        // Same channel ChangeTenantStatusCommandHandler uses for Suspended/Archived - the
        // frontend's realtime handler distinguishes PendingLimitSelection from those (routes to
        // the reconciliation wizard, does not force a logout - see RealtimeContext.tsx).
        await _realtimeService.NotifyTenantStatusChangedAsync(tenantId, TenantStatus.PendingLimitSelection.ToString());

        return LimitReconciliationOutcome.Opened;
    }

    private async Task<LimitReconciliationOutcome> ReconcilePendingAsync(Tenant tenant, CancellationToken ct)
    {
        var reconciliation = await _tenantRepository.GetOpenReconciliationAsync(tenant.Id, ct);
        if (reconciliation is null)
            return LimitReconciliationOutcome.NoChange;

        var overLimits = await GetOverLimitsAsync(tenant.Id, ct);

        if (overLimits.Count == 0)
        {
            // The new numbers (a plan change in either direction, a limit edit, or an override
            // change) cover everything already in use - nothing left for the tenant to
            // hand-pick, so close the reconciliation and lift the lock automatically rather than
            // making them run the wizard just to reselect what they already have.
            reconciliation.CompletedAt = DateTime.UtcNow;
            reconciliation.CompletedByUserId = _userContext.UserId;
            tenant.Status = TenantStatus.Active;

            await _unitOfWork.SaveChangesAsync(ct);

            _tenantStatusCache.Invalidate(tenant.Id);
            await _realtimeService.NotifyTenantStatusChangedAsync(tenant.Id, TenantStatus.Active.ToString());
            return LimitReconciliationOutcome.Resolved;
        }

        // Still over on at least one resource - the tenant stays locked, but the set of
        // over-limit resources may have shifted (e.g. branches now fit but users still don't), so
        // refresh the snapshot the wizard reads rather than leaving it reflecting the old numbers.
        // TriggeredByPlanId is deliberately left as whatever it was when this reconciliation
        // first opened - it's an audit field ("what plan was in effect when this window opened"),
        // not a "most recently seen" field, so it's never recomputed here.
        reconciliation.RequiredResourcesJson = JsonSerializer.Serialize(overLimits.ToDictionary(l => l.ResourceKey, l => l.Used));
        await _unitOfWork.SaveChangesAsync(ct);
        return LimitReconciliationOutcome.StillPending;
    }

    private async Task<List<EffectiveLimit>> GetOverLimitsAsync(Guid tenantId, CancellationToken ct)
    {
        var limits = await _limitService.GetAllAsync(tenantId, ct);
        return limits.Where(l => !l.HasHeadroom && LimitedResources.RequiresSelection(l.ResourceKey)).ToList();
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

        // Same ordering fix as ReconcileAsync - commit before invalidate/notify.
        await _unitOfWork.SaveChangesAsync(ct);

        _tenantStatusCache.Invalidate(tenantId);
        await _realtimeService.NotifyTenantStatusChangedAsync(tenantId, TenantStatus.PendingLimitSelection.ToString());
    }
}
