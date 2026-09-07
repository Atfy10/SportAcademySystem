using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.Events;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Closes enrollments whose subscription expired and was never renewed. A trainee keeps their
    /// place in the group for a week after expiry - nothing changes during that window, and
    /// renewing inside it carries the same enrollment forward untouched (see
    /// SubscriptionCreationService). Once the week passes, they've left: EndDate is stamped,
    /// IsActive goes false, and the group slot frees up for someone else.
    /// </summary>
    public class EnrollmentLapseService : BackgroundService
    {
        /// <summary>
        /// How long after a subscription expires a trainee keeps their spot before the
        /// enrollment is closed.
        /// </summary>
        public const int GracePeriodDays = 7;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EnrollmentLapseService> _logger;

        public EnrollmentLapseService(IServiceScopeFactory scopeFactory, ILogger<EnrollmentLapseService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Enrollment lapse service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                    await CloseLapsedEnrollmentsAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during enrollment lapse sweep");
                }
            }

            _logger.LogInformation("Enrollment lapse service stopped");
        }

        private async Task CloseLapsedEnrollmentsAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var cutoff = DateTime.UtcNow.AddDays(-GracePeriodDays);

            // IgnoreQueryFilters is required, not optional: every business entity carries a
            // tenant query filter fed by an AsyncLocal that only middleware sets, so a
            // background sweep running outside any request has no tenant and would otherwise
            // match zero rows in every tenant. The tenant is instead read off each row below.
            //
            // Excludes any tenant that isn't Active: a customer whose account is suspended
            // shouldn't come back to find weeks of enrollments auto-closed while they were
            // locked out and couldn't renew anything.
            var lapsed = await context.Set<Enrollment>()
                .IgnoreQueryFilters()
                .Where(e => !e.IsDeleted)
                .Where(e => e.EndDate == null)
                .Where(e => e.ExpiryDate < cutoff)
                .Where(e => context.Set<Tenant>().Any(t => t.Id == e.TenantId && t.Status == TenantStatus.Active))
                .ToListAsync(ct);

            if (lapsed.Count == 0)
                return;

            foreach (var enrollment in lapsed)
            {
                // Stamped from the expiry date, not from "now" - the trainee left the day their
                // grace period ran out, whichever day this sweep happens to run.
                enrollment.EndDate = enrollment.ExpiryDate.AddDays(GracePeriodDays);
                enrollment.IsActive = false;
            }

            await context.SaveChangesAsync(ct);

            _logger.LogInformation("Closed {Count} lapsed enrollments", lapsed.Count);

            await NotifyPerTenantAsync(scope, lapsed, ct);
        }

        // Notifications are tenant-scoped (they're persisted against a tenant and pushed to that
        // academy's staff), so each tenant's batch is published with that tenant set as the
        // ambient context. Best-effort by design: the enrollments are already closed and saved,
        // and a notification failure must not make the sweep look like it failed or cause it to
        // reprocess rows it has already handled.
        private async Task NotifyPerTenantAsync(IServiceScope scope, List<Enrollment> lapsed, CancellationToken ct)
        {
            var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantIdProvider>();
            var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
            var originalTenantId = tenantProvider.TenantId;

            try
            {
                foreach (var tenantGroup in lapsed.GroupBy(e => e.TenantId))
                {
                    try
                    {
                        tenantProvider.SetTenantId(tenantGroup.Key);

                        foreach (var enrollment in tenantGroup)
                        {
                            await publisher.Publish(
                                new EnrollmentLifecycleEvent(enrollment.Id, "Ended", "System"), ct);
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex,
                            "Failed to notify tenant {TenantId} about {Count} closed enrollments - " +
                            "the enrollments themselves were still closed successfully.",
                            tenantGroup.Key, tenantGroup.Count());
                    }
                }
            }
            finally
            {
                tenantProvider.SetTenantId(originalTenantId);
            }
        }
    }
}
