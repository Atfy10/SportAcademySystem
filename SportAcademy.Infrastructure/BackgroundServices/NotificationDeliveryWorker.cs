using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Polls the NotificationDelivery outbox and sends every due Email/Push/WhatsApp row through
    /// its matching INotificationChannelSender (resolved by .Channel). InApp never appears here -
    /// it's written as Sent immediately by NotificationChannelDispatcher, never queued. Runs
    /// outside any request (no ambient tenant), so every claimed batch is grouped by TenantId and
    /// processed under ITenantIdProvider.Impersonate(tenantId) - same reasoning as
    /// SessionOccurrenceCompletionService: a channel sender resolving contact info through
    /// IContactResolver needs a real ambient tenant for its tenant-scoped queries to return
    /// anything.
    /// </summary>
    public class NotificationDeliveryWorker : BackgroundService
    {
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);
        private const int BatchSize = 200;
        private const int MaxAttempts = 5;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationDeliveryWorker> _logger;

        public NotificationDeliveryWorker(IServiceScopeFactory scopeFactory, ILogger<NotificationDeliveryWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification delivery worker started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessDueDeliveriesAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during notification delivery sweep");
                }

                try
                {
                    await Task.Delay(PollInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("Notification delivery worker stopped");
        }

        private async Task ProcessDueDeliveriesAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantIdProvider>();
            var senders = scope.ServiceProvider.GetRequiredService<IEnumerable<INotificationChannelSender>>()
                .ToDictionary(s => s.Channel);

            var now = DateTime.UtcNow;

            // No ambient tenant here - IgnoreQueryFilters is required to claim due rows across
            // every tenant in one pass, same reasoning as every other cross-tenant sweep in this
            // folder.
            var due = await context.Set<NotificationDelivery>()
                .IgnoreQueryFilters()
                .Where(d => d.Status == NotificationDeliveryStatus.Pending && d.NextAttemptAt <= now)
                .OrderBy(d => d.NextAttemptAt)
                .Take(BatchSize)
                .Include(d => d.Notification)
                .ToListAsync(ct);

            if (due.Count == 0) return;

            foreach (var tenantGroup in due.GroupBy(d => d.TenantId))
            {
                using (tenantProvider.Impersonate(tenantGroup.Key))
                {
                    foreach (var delivery in tenantGroup)
                    {
                        if (!senders.TryGetValue(delivery.Channel, out var sender))
                        {
                            _logger.LogWarning(
                                "No INotificationChannelSender registered for channel {Channel} - skipping delivery {DeliveryId}.",
                                delivery.Channel, delivery.Id);
                            delivery.Status = NotificationDeliveryStatus.Skipped;
                            continue;
                        }

                        delivery.AttemptCount++;

                        ChannelSendResult result;
                        try
                        {
                            result = await sender.SendAsync(delivery, delivery.Notification, ct);
                        }
                        catch (Exception ex) when (ex is not OperationCanceledException)
                        {
                            result = new ChannelSendResult(false, true, ex.Message);
                        }

                        if (result.Succeeded)
                        {
                            delivery.Status = NotificationDeliveryStatus.Sent;
                            delivery.SentAt = DateTime.UtcNow;
                            delivery.LastError = null;
                        }
                        else if (!result.ShouldRetry)
                        {
                            delivery.Status = NotificationDeliveryStatus.Skipped;
                            delivery.LastError = result.Error;
                        }
                        else if (delivery.AttemptCount >= MaxAttempts)
                        {
                            delivery.Status = NotificationDeliveryStatus.DeadLettered;
                            delivery.LastError = result.Error;
                            _logger.LogError(
                                "Notification delivery {DeliveryId} (channel {Channel}) dead-lettered after {Attempts} attempts: {Error}",
                                delivery.Id, delivery.Channel, delivery.AttemptCount, result.Error);
                        }
                        else
                        {
                            // Exponential backoff: 2^AttemptCount minutes (1, 2, 4, 8, 16).
                            delivery.Status = NotificationDeliveryStatus.Pending;
                            delivery.NextAttemptAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, delivery.AttemptCount));
                            delivery.LastError = result.Error;
                        }
                    }

                    // Impersonate above already set a real ambient TenantId matching every row
                    // in this group, so the interceptor's tenant check is satisfied without
                    // needing AllowCrossTenantOperation - that's only for the no-ambient-tenant
                    // case.
                    await context.SaveChangesAsync(ct);
                }
            }

            _logger.LogInformation("Processed {Count} due notification deliveries", due.Count);
        }
    }
}
