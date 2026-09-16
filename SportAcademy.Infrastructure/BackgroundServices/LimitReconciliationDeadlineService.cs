using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Enforces the 7-day deadline on a downgrade-reconciliation window (see
    /// LimitReconciliationService): a tenant that hasn't completed its forced branch/sport/user
    /// selection by TenantLimitReconciliation.DeadlineAt is suspended - the existing suspension
    /// machinery (TenantStatusGuardMiddleware, SignalR force-disconnect via
    /// ChangeTenantStatusCommandHandler's own path) then takes over unchanged. Same 24h-loop
    /// shape as EnrollmentLapseService/TenantArchivalService.
    /// </summary>
    public class LimitReconciliationDeadlineService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<LimitReconciliationDeadlineService> _logger;

        public LimitReconciliationDeadlineService(
            IServiceScopeFactory scopeFactory, ILogger<LimitReconciliationDeadlineService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Limit reconciliation deadline service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                    await SuspendLapsedReconciliationsAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during limit reconciliation deadline sweep");
                }
            }

            _logger.LogInformation("Limit reconciliation deadline service stopped");
        }

        private async Task SuspendLapsedReconciliationsAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var tenantStatusCache = scope.ServiceProvider.GetRequiredService<ITenantStatusCacheInvalidator>();
            var realtimeService = scope.ServiceProvider.GetRequiredService<IRealtimeService>();

            // TenantLimitReconciliation and Tenant are both deliberately NOT ITenantScoped (see
            // each entity's own comment), so - unlike EnrollmentLapseService's identical-shaped
            // sweep over Enrollment - no IgnoreQueryFilters() is needed here: neither entity is
            // ever filtered by an ambient tenant to begin with.
            var lapsed = await context.Set<TenantLimitReconciliation>()
                .Where(r => r.CompletedAt == null && r.DeadlineAt < DateTime.UtcNow)
                .Include(r => r.Tenant)
                .ToListAsync(ct);

            // Only a tenant still actually PendingLimitSelection is suspended - one a SuperAdmin
            // already moved out of that state some other way (e.g. archived it directly) has
            // nothing left for this sweep to do, and TenantStatusPolicy wouldn't allow the
            // transition anyway.
            var toSuspend = lapsed.Where(r => r.Tenant.Status == TenantStatus.PendingLimitSelection).ToList();
            if (toSuspend.Count == 0)
                return;

            foreach (var reconciliation in toSuspend)
            {
                reconciliation.Tenant.Status = TenantStatus.Suspended;
            }

            await context.SaveChangesAsync(ct);

            foreach (var reconciliation in toSuspend)
            {
                tenantStatusCache.Invalidate(reconciliation.TenantId);
                try
                {
                    await realtimeService.NotifyTenantStatusChangedAsync(reconciliation.TenantId, TenantStatus.Suspended.ToString());
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // The suspension itself already committed - a failed realtime push just
                    // means an already-open session doesn't hear about it until its next
                    // request hits TenantStatusGuardMiddleware, same as any other missed push.
                    _logger.LogError(ex, "Failed to notify tenant {TenantId} of suspension after a lapsed reconciliation deadline.",
                        reconciliation.TenantId);
                }
            }

            _logger.LogInformation("Suspended {Count} tenant(s) whose limit-reconciliation deadline lapsed.", toSuspend.Count);
        }
    }
}
