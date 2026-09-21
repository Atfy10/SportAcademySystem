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
using SportAcademy.Domain.Services;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Flips a Scheduled session occurrence to Completed once its attendance window has closed
    /// (midnight at the end of the session's own day - see AttendanceWindow, the same rule
    /// CreateAttendanceCommandHandler/BulkCreateAttendanceCommandHandler enforce) - nothing else
    /// in the codebase ever sets this
    /// automatically, so without this sweep every occurrence sits at Scheduled forever unless a
    /// coach manually updates it via SessionOccurrencesNearbyModal. "Completed" is what the
    /// attendance handlers now check to reject a mark attempt outright (in addition to their own
    /// time-window check, which covers a session this sweep hasn't caught up to yet).
    /// </summary>
    public class SessionOccurrenceCompletionService : BackgroundService
    {
        /// <summary>How often the sweep runs - short, unlike the 24h sweeps elsewhere in this
        /// folder, because "has this session's window closed" is itself a small, fast-changing
        /// window (attendance is time-sensitive; a session shouldn't read as Scheduled for hours
        /// after it's genuinely over).</summary>
        private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(10);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SessionOccurrenceCompletionService> _logger;

        public SessionOccurrenceCompletionService(IServiceScopeFactory scopeFactory, ILogger<SessionOccurrenceCompletionService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Session occurrence completion service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CompleteDueSessionsAsync(stoppingToken);
                    await Task.Delay(PollInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during session occurrence completion sweep");
                }
            }

            _logger.LogInformation("Session occurrence completion service stopped");
        }

        private async Task CompleteDueSessionsAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantIdProvider>();
            var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

            var utcNow = DateTime.UtcNow;

            // IgnoreQueryFilters is required, not optional: this sweep runs outside any request,
            // with no ambient tenant to satisfy the global tenant filter - same reasoning as
            // EnrollmentLapseService. SessionOccurrence.StartDateTime is written and compared as
            // the OWNING TENANT's own wall-clock time throughout this codebase (see ITenantClock's
            // remarks), never UTC, so "is this session over yet" can't be answered with one global
            // UtcNow - it has to be resolved per tenant, below. The StartDateTime < utcNow.AddHours(15)
            // clause is a loose SQL-level pre-filter only (no real timezone runs further than
            // UTC-12/UTC+14 from UTC), so this doesn't load every future-scheduled session in the
            // system on each pass - just ones that could plausibly already be due somewhere.
            var candidates = await context.Set<SessionOccurrence>()
                .IgnoreQueryFilters()
                .Where(s => s.Status == SessionStatus.Scheduled)
                .Where(s => s.StartDateTime < utcNow.AddHours(15))
                .Select(s => new
                {
                    Occurrence = s,
                    s.TenantId,
                })
                .ToListAsync(ct);

            if (candidates.Count == 0)
                return;

            var tenantIds = candidates.Select(c => c.TenantId).Distinct().ToList();

            // Excludes any tenant that isn't Active, mirroring EnrollmentLapseService - a
            // suspended tenant's data shouldn't silently change underneath it while it's locked
            // out and can't see or contest it.
            var tenantTimeZones = await context.Set<Tenant>()
                .IgnoreQueryFilters()
                .Where(t => tenantIds.Contains(t.Id) && t.Status == TenantStatus.Active)
                .Join(context.Set<TenantSettings>(), t => t.Id, ts => ts.TenantId, (t, ts) => new { t.Id, ts.TimeZone })
                .ToDictionaryAsync(x => x.Id, x => x.TimeZone, ct);

            var completed = new List<SessionOccurrence>();

            foreach (var tenantGroup in candidates.GroupBy(c => c.TenantId))
            {
                if (!tenantTimeZones.TryGetValue(tenantGroup.Key, out var timeZoneId))
                    continue; // tenant not Active, or has no settings row yet - skip, never guess.

                var tenantNow = ResolveTenantNow(utcNow, timeZoneId);

                foreach (var candidate in tenantGroup)
                {
                    var windowCloses = AttendanceWindow.ClosesAt(candidate.Occurrence.StartDateTime);

                    if (tenantNow < windowCloses)
                        continue;

                    candidate.Occurrence.Status = SessionStatus.Completed;
                    completed.Add(candidate.Occurrence);
                }
            }

            if (completed.Count == 0)
                return;

            // Every row here already carries its own real, non-empty TenantId from the query
            // above - this scope only lets the save through, it never invents a tenant for a row
            // that doesn't already have one.
            using (tenantProvider.AllowCrossTenantOperation())
            {
                await context.SaveChangesAsync(ct);
            }

            _logger.LogInformation("Completed {Count} session occurrences", completed.Count);

            foreach (var occurrence in completed)
            {
                try
                {
                    await publisher.Publish(new SessionOccurrenceUpdatedEvent(occurrence.Id), ct);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex,
                        "Failed to publish SessionOccurrenceUpdatedEvent for {Id} - the occurrence itself was still completed successfully.",
                        occurrence.Id);
                }
            }
        }

        private static DateTime ResolveTenantNow(DateTime utcNow, string timeZoneId)
        {
            if (string.IsNullOrWhiteSpace(timeZoneId))
                return utcNow;

            try
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                return TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);
            }
            catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
            {
                // Same fallback as ITenantClock: an invalid/unrecognized IANA id must not stall
                // completion for every session on this tenant indefinitely.
                return utcNow;
            }
        }
    }
}
