using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Services;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Keeps stored subscription state in step with the calendar:
    /// <list type="number">
    /// <item>Active subscriptions whose end date has passed are stamped Expired (this used to
    /// happen only as a side effect of someone opening the subscription stats).</item>
    /// <item>A renewal sold ahead of time takes over the trainee's group enrollment on its start
    /// date. SubscriptionCreationService deliberately leaves the enrollment on the current
    /// subscription when the renewal starts in the future, so the trainee keeps using the
    /// sessions they already paid for until then; this is the other half of that hand-over.</item>
    /// </list>
    /// Reads always derive Upcoming/Expired from the dates anyway (SubscriptionBilling), so this
    /// running late never shows a wrong badge - it only keeps the stored rows and the enrollment
    /// honest.
    /// </summary>
    public class SubscriptionLifecycleService : BackgroundService
    {
        private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan Interval = TimeSpan.FromHours(3);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SubscriptionLifecycleService> _logger;

        public SubscriptionLifecycleService(IServiceScopeFactory scopeFactory, ILogger<SubscriptionLifecycleService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Subscription lifecycle service started");

            try
            {
                await Task.Delay(StartupDelay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SweepAsync(stoppingToken);
                    await Task.Delay(Interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during subscription lifecycle sweep");
                    try { await Task.Delay(Interval, stoppingToken); }
                    catch (OperationCanceledException) { break; }
                }
            }

            _logger.LogInformation("Subscription lifecycle service stopped");
        }

        public async Task SweepAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantIdProvider>();
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var handedOver = await HandOverStartedRenewalsAsync(context, tenantProvider, today, ct);

            // After the hand-over, so a renewal starting today has already taken the enrollment
            // before the subscription it replaces is (also) expired by date below.
            int expired;
            using (tenantProvider.AllowCrossTenantOperation())
            {
                expired = await context.SubscriptionDetails
                    .IgnoreQueryFilters()
                    .Where(sd => !sd.IsDeleted && sd.Status == SubscriptionStatus.Active && sd.EndDate < today)
                    .ExecuteUpdateAsync(s => s.SetProperty(sd => sd.Status, SubscriptionStatus.Expired), ct);
            }

            if (handedOver > 0 || expired > 0)
                _logger.LogInformation("Subscription lifecycle: {HandedOver} renewals took over their enrollment, {Expired} subscriptions expired",
                    handedOver, expired);
        }

        private static async Task<int> HandOverStartedRenewalsAsync(
            ApplicationDbContext context, ITenantIdProvider tenantProvider, DateOnly today, CancellationToken ct)
        {
            // Started, still running, and not yet backing any enrollment - but the trainee has an
            // open enrollment in the same sport that still points at an EARLIER subscription.
            // That's exactly a renewal waiting for its hand-over; a first-time subscription has
            // no such enrollment and is left for staff to enroll as usual.
            var pending = await context.SubscriptionDetails
                .IgnoreQueryFilters()
                .Where(sd => !sd.IsDeleted
                    && sd.Status == SubscriptionStatus.Active
                    && sd.StartDate <= today
                    && sd.EndDate >= today
                    && !context.Enrollments.IgnoreQueryFilters().Any(e => e.SubscriptionDetailsId == sd.Id && !e.IsDeleted))
                .Select(sd => new
                {
                    Subscription = sd,
                    sd.SportPrice.SportSubscriptionType.SubscriptionType.DaysPerMonth,
                    sd.SportPrice.SportSubscriptionType.SubscriptionType.NumberOfMonths,
                    Enrollment = context.Enrollments.IgnoreQueryFilters()
                        .Where(e => !e.IsDeleted
                            && e.TenantId == sd.TenantId
                            && e.TraineeId == sd.TraineeId
                            && e.EndDate == null
                            && e.TraineeGroup.Coach.SportId == sd.SportId
                            && e.SubscriptionDetailsId != sd.Id
                            && e.SubscriptionDetails.StartDate < sd.StartDate)
                        .OrderByDescending(e => e.EnrollmentDate)
                        .FirstOrDefault(),
                })
                .Where(x => x.Enrollment != null)
                .ToListAsync(ct);

            if (pending.Count == 0)
                return 0;

            var oldSubscriptionIds = pending.Select(p => p.Enrollment!.SubscriptionDetailsId).Distinct().ToList();
            var oldSubscriptions = await context.SubscriptionDetails
                .IgnoreQueryFilters()
                .Where(sd => oldSubscriptionIds.Contains(sd.Id))
                .ToDictionaryAsync(sd => sd.Id, ct);

            foreach (var item in pending)
            {
                var enrollment = item.Enrollment!;
                var renewal = item.Subscription;
                var totalSessions = TrainingScheduleService.CalculateTotalSessions(item.DaysPerMonth, item.NumberOfMonths);

                if (oldSubscriptions.TryGetValue(enrollment.SubscriptionDetailsId, out var old)
                    && old.Status == SubscriptionStatus.Active)
                    old.Status = SubscriptionStatus.Expired;

                // Same carry-forward SubscriptionCreationService does for a renewal that starts
                // on the day it's sold.
                enrollment.SubscriptionDetailsId = renewal.Id;
                enrollment.SessionAllowed = totalSessions;
                enrollment.SessionRemaining = totalSessions;
                enrollment.ExpiryDate = renewal.EndDate.ToDateTime(TimeOnly.MinValue);
                enrollment.Status = EnrollmentStatus.Active;
            }

            // Every row carries its own real TenantId from the query - see EnrollmentLapseService.
            using (tenantProvider.AllowCrossTenantOperation())
            {
                await context.SaveChangesAsync(ct);
            }

            return pending.Count;
        }
    }
}
