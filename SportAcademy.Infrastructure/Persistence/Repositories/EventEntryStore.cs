using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Events;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class EventEntryStore : IEventEntryStore
    {
        // Enough for a crowd arriving together; each retry means another scan won the race.
        private const int MaxAttempts = 8;

        private readonly ApplicationDbContext _context;

        public EventEntryStore(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EventEntryTarget?> FindByTokenAsync(string token, string lang, CancellationToken ct = default)
            // No tenant yet - the code is what identifies the event (and so the academy).
            => await _context.Events.IgnoreQueryFilters()
                .AsNoTracking()
                .Where(e => e.EntryToken == token && !e.IsDeleted)
                .Select(e => new EventEntryTarget(
                    e.Id,
                    e.TenantId,
                    e.Tenant.Status,
                    e.Tenant.DisplayName,
                    e.Title,
                    e.Branch.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? e.Branch.Name,
                    e.IsCancelled,
                    e.StartsAt,
                    e.EndsAt,
                    e.Capacity,
                    e.AdmittedCount))
                .SingleOrDefaultAsync(ct);

        public async Task<EventAdmission?> FindAdmissionAsync(int eventId, string deviceKey, CancellationToken ct = default)
            => await _context.Set<EventAdmission>().AsNoTracking()
                .SingleOrDefaultAsync(a => a.EventId == eventId && a.DeviceKey == deviceKey, ct);

        public async Task<AdmitOutcome> TryAdmitAsync(int eventId, string deviceKey, CancellationToken ct = default)
        {
            for (var attempt = 0; attempt < MaxAttempts; attempt++)
            {
                _context.ChangeTracker.Clear();
                var ev = await _context.Events.SingleAsync(e => e.Id == eventId, ct);
                if (ev.AdmittedCount >= ev.Capacity)
                    return new AdmitOutcome(EventEntryResult.Full, ev.AdmittedCount, null);

                // Count and admission are saved together, guarded by the event's rowversion: if
                // another scan got in first, this save fails and we look again at the new count.
                ev.AdmittedCount++;
                var admission = new EventAdmission
                {
                    EventId = ev.Id,
                    DeviceKey = deviceKey,
                    Number = ev.AdmittedCount,
                    AdmittedAt = DateTime.UtcNow,
                };
                _context.Set<EventAdmission>().Add(admission);

                try
                {
                    await _context.SaveChangesAsync(ct);
                    return new AdmitOutcome(EventEntryResult.Admitted, ev.AdmittedCount, admission);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Someone else was let in at the same moment - try again with the new count.
                }
                catch (DbUpdateException)
                {
                    // The unique (event, device) index: this same phone was let in by a
                    // simultaneous scan of its own.
                    _context.ChangeTracker.Clear();
                    var existing = await FindAdmissionAsync(eventId, deviceKey, ct);
                    if (existing is null) throw;
                    var count = await _context.Events.Where(e => e.Id == eventId).Select(e => e.AdmittedCount).SingleAsync(ct);
                    return new AdmitOutcome(EventEntryResult.AlreadyAdmitted, count, existing);
                }
            }

            throw new DbUpdateConcurrencyException("Too many people scanned at the same moment. Please scan again.");
        }
    }
}
