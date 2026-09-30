using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Helpers;
using SportAcademy.Infrastructure.Persistence.DBContext;
using System.Globalization;

namespace SportAcademy.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Warns the academy about money it's still owed on subscriptions sold with a deposit. Once
    /// a few days before the collect date ("due soon") and once when it passes unpaid
    /// ("overdue"), each as one notification per academy to its Owners, Admins and Accountants,
    /// linking to the Outstanding page. Warn-only by design: nothing is suspended or blocked.
    /// Each invoice is announced at most once per stage (Invoice.DueSoonNotifiedOn /
    /// OverdueNotifiedOn), so running more often than daily - or restarting - never repeats an
    /// alert; moving the due date (a refund reopening the balance) clears both stamps.
    /// </summary>
    public class OverdueBalanceService : BackgroundService
    {
        public const int DueSoonDays = 3;
        private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan Interval = TimeSpan.FromHours(6);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OverdueBalanceService> _logger;

        public OverdueBalanceService(IServiceScopeFactory scopeFactory, ILogger<OverdueBalanceService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Overdue balance service started");

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
                    _logger.LogError(ex, "Error during overdue balance sweep");
                    try { await Task.Delay(Interval, stoppingToken); }
                    catch (OperationCanceledException) { break; }
                }
            }

            _logger.LogInformation("Overdue balance service stopped");
        }

        private sealed record OwedRow(int InvoiceId, Guid TenantId, string TraineeName, decimal Outstanding, string Currency, DateOnly DueDate);

        public async Task SweepAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantIdProvider>();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var soonLimit = today.AddDays(DueSoonDays);

            // IgnoreQueryFilters: no request, so no ambient tenant - see EnrollmentLapseService.
            // Only Active tenants: a suspended academy can't act on the alert anyway.
            var owed = context.Set<Invoice>()
                .IgnoreQueryFilters()
                .Where(i => !i.IsDeleted
                    && i.Status != InvoiceStatus.Cancelled
                    && i.AmountPaid < i.GrandTotal)
                .Where(i => context.Set<Tenant>().Any(t => t.Id == i.TenantId && t.Status == TenantStatus.Active));

            var overdue = await Project(owed.Where(i => i.DueDate < today && i.OverdueNotifiedOn == null))
                .ToListAsync(ct);
            var dueSoon = await Project(owed.Where(i => i.DueDate >= today && i.DueDate <= soonLimit && i.DueSoonNotifiedOn == null))
                .ToListAsync(ct);

            if (overdue.Count == 0 && dueSoon.Count == 0)
                return;

            var overdueIds = overdue.Select(r => r.InvoiceId).ToList();
            var dueSoonIds = dueSoon.Select(r => r.InvoiceId).ToList();

            // Stamp first, then notify: a crash between the two loses one alert rather than
            // repeating it forever.
            using (tenantProvider.AllowCrossTenantOperation())
            {
                if (overdueIds.Count > 0)
                    await context.Set<Invoice>().IgnoreQueryFilters()
                        .Where(i => overdueIds.Contains(i.Id))
                        .ExecuteUpdateAsync(s => s.SetProperty(i => i.OverdueNotifiedOn, today), ct);

                if (dueSoonIds.Count > 0)
                    await context.Set<Invoice>().IgnoreQueryFilters()
                        .Where(i => dueSoonIds.Contains(i.Id))
                        .ExecuteUpdateAsync(s => s.SetProperty(i => i.DueSoonNotifiedOn, today), ct);
            }

            _logger.LogInformation("Overdue sweep: {Overdue} overdue and {DueSoon} due-soon balances to announce",
                overdue.Count, dueSoon.Count);

            await NotifyAsync(scope, overdue, isOverdue: true, ct);
            await NotifyAsync(scope, dueSoon, isOverdue: false, ct);
        }

        private static IQueryable<OwedRow> Project(IQueryable<Invoice> invoices)
            => invoices.Select(i => new OwedRow(
                i.Id,
                i.TenantId,
                i.Trainee != null ? i.Trainee.FirstName + " " + i.Trainee.LastName : i.InvoiceNumber,
                i.GrandTotal - i.AmountPaid,
                i.Currency,
                i.DueDate));

        // Best-effort, per tenant with that tenant as the ambient context (notifications are
        // tenant-scoped) - same shape as EnrollmentLapseService.NotifyPerTenantAsync.
        private async Task NotifyAsync(IServiceScope scope, List<OwedRow> rows, bool isOverdue, CancellationToken ct)
        {
            if (rows.Count == 0) return;

            var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantIdProvider>();
            var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var originalTenantId = tenantProvider.TenantId;

            try
            {
                foreach (var tenantRows in rows.GroupBy(r => r.TenantId))
                {
                    try
                    {
                        tenantProvider.SetTenantId(tenantRows.Key);
                        var (title, message) = Compose(tenantRows.ToList(), isOverdue);

                        await notifications.SendNotificationToGroupsWithLinkAsync(
                            isOverdue ? NotificationEventTypes.PaymentOverdue : NotificationEventTypes.PaymentDueSoon,
                            [NotificationGroupNames.Owners, NotificationGroupNames.Admins, NotificationGroupNames.Accountants],
                            title,
                            message,
                            isOverdue ? NotificationType.Warning : NotificationType.Info,
                            isOverdue ? "/finance/outstanding?overdue=1" : "/finance/outstanding");
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex, "Failed to send balance alert to tenant {TenantId}", tenantRows.Key);
                    }
                }
            }
            finally
            {
                tenantProvider.SetTenantId(originalTenantId);
            }
        }

        // Names the trainees when there are only a few, so the alert is actionable on its own.
        private static (string Title, string Message) Compose(List<OwedRow> rows, bool isOverdue)
        {
            static string Money(decimal amount, string currency)
                => $"{currency} {amount.ToString("N3", CultureInfo.InvariantCulture)}";

            var total = rows.Sum(r => r.Outstanding);
            var currency = rows[0].Currency;
            var detail = rows.Count <= 3
                ? string.Join(", ", rows.Select(r => $"{r.TraineeName} ({Money(r.Outstanding, r.Currency)}, due {r.DueDate:yyyy-MM-dd})"))
                : $"{rows.Count} trainees";

            return isOverdue
                ? (rows.Count == 1 ? "Balance overdue" : "Balances overdue",
                   $"Past the collect date and still unpaid: {detail}. Total outstanding {Money(total, currency)}.")
                : (rows.Count == 1 ? "Balance due soon" : "Balances due soon",
                   $"Due within {DueSoonDays} days: {detail}. Total {Money(total, currency)}.");
        }
    }
}
