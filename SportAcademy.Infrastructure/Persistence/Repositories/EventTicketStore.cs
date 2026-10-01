using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Events;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class EventTicketStore : IEventTicketStore
    {
        private readonly ApplicationDbContext _context;

        public EventTicketStore(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PublicTicketTarget?> FindPublicAsync(string token, string lang, CancellationToken ct = default)
            // No tenant yet - the code is what identifies the ticket (and so the academy).
            => await _context.EventTickets.IgnoreQueryFilters()
                .AsNoTracking()
                .Where(t => t.Token == token && !t.Event.IsDeleted)
                .Select(t => new PublicTicketTarget(
                    t.TenantId,
                    t.Event.Tenant.Status,
                    t.Event.Tenant.DisplayName,
                    t.Event.Title,
                    t.Event.Branch.Translations.Where(b => b.LangCode == lang).Select(b => b.Name).FirstOrDefault() ?? t.Event.Branch.Name,
                    t.Event.IsCancelled,
                    t.Event.StartsAt,
                    t.Event.EndsAt,
                    t.Number,
                    t.GuestName,
                    t.AdmittedAt))
                .SingleOrDefaultAsync(ct);

        public async Task<EventTicket?> FindByTokenAsync(string token, CancellationToken ct = default)
            => await WithEvent().SingleOrDefaultAsync(t => t.Token == token, ct);

        public async Task<EventTicket?> FindByNumberAsync(int eventId, int number, CancellationToken ct = default)
            => await WithEvent().SingleOrDefaultAsync(t => t.EventId == eventId && t.Number == number, ct);

        public async Task<EventTicket?> GetForUpdateAsync(int ticketId, CancellationToken ct = default)
            => await _context.EventTickets
                .Include(t => t.Event)
                .SingleOrDefaultAsync(t => t.Id == ticketId, ct);

        public async Task<EventTicketCounts> GetCountsAsync(int eventId, CancellationToken ct = default)
        {
            // One query for both numbers (the scanner asks after every admit).
            var counts = await _context.Events.AsNoTracking()
                .Where(e => e.Id == eventId)
                .Select(e => new { Issued = e.Tickets.Count(), Admitted = e.Tickets.Count(t => t.AdmittedAt != null) })
                .SingleOrDefaultAsync(ct);
            return counts is null ? new EventTicketCounts(0, 0) : new EventTicketCounts(counts.Issued, counts.Admitted);
        }

        public async Task<List<int>> GetNumbersAsync(int eventId, CancellationToken ct = default)
            => await _context.EventTickets.AsNoTracking()
                .Where(t => t.EventId == eventId)
                .Select(t => t.Number)
                .ToListAsync(ct);

        public async Task<(List<EventTicket> Items, int TotalCount)> GetPagedAsync(
            int eventId, EventTicketFilter filter, string? term, PageRequest page, CancellationToken ct = default)
        {
            var query = _context.EventTickets.AsNoTracking().Where(t => t.EventId == eventId);

            query = filter switch
            {
                EventTicketFilter.Unused => query.Where(t => t.AdmittedAt == null),
                EventTicketFilter.Admitted => query.Where(t => t.AdmittedAt != null),
                _ => query,
            };

            // A number finds that ticket; anything else searches guest names.
            if (!string.IsNullOrWhiteSpace(term))
            {
                var trimmed = term.Trim().TrimStart('#');
                query = int.TryParse(trimmed, out var number)
                    ? query.Where(t => t.Number == number)
                    : query.Where(t => t.GuestName != null && t.GuestName.Contains(trimmed));
            }

            var totalCount = await query.CountAsync(ct);
            // Numbers are random, so list tickets in the order they were issued.
            var items = await query
                .OrderBy(t => t.IssuedAt)
                .ThenBy(t => t.Id)
                .Skip(page.Skip)
                .Take(page.PageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task AddRangeAsync(IEnumerable<EventTicket> tickets, CancellationToken ct = default)
        {
            await _context.EventTickets.AddRangeAsync(tickets, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task RemoveAsync(EventTicket ticket, CancellationToken ct = default)
        {
            _context.EventTickets.Remove(ticket);
            await _context.SaveChangesAsync(ct);
        }

        public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);

        public async Task<bool> TryAdmitAsync(int ticketId, Guid admittedByUserId, DateTime nowUtc, CancellationToken ct = default)
        {
            var ticket = await _context.EventTickets.SingleOrDefaultAsync(t => t.Id == ticketId, ct);
            if (ticket is null || ticket.AdmittedAt is not null)
                return false;

            ticket.AdmittedAt = nowUtc;
            ticket.AdmittedByUserId = admittedByUserId;

            try
            {
                // Guarded by the ticket's rowversion: if another doorman admitted it first, this
                // save fails instead of admitting it twice.
                await _context.SaveChangesAsync(ct);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                _context.Entry(ticket).State = EntityState.Detached;
                return false;
            }
        }

        public async Task<List<CheckInEventRow>> GetCheckInEventsAsync(
            DateTime nowUtc, DateTime untilUtc, string lang, CancellationToken ct = default)
            => await _context.Events.AsNoTracking()
                .Where(e => !e.IsCancelled && e.EndsAt > nowUtc && e.StartsAt < untilUtc)
                .OrderBy(e => e.StartsAt)
                .ThenBy(e => e.Id)
                .Select(e => new CheckInEventRow(
                    e.Id,
                    e.Title,
                    e.Branch.Translations.Where(b => b.LangCode == lang).Select(b => b.Name).FirstOrDefault() ?? e.Branch.Name,
                    e.StartsAt,
                    e.EndsAt,
                    e.Capacity,
                    e.Tickets.Count(),
                    e.Tickets.Count(t => t.AdmittedAt != null)))
                .ToListAsync(ct);

        private IQueryable<EventTicket> WithEvent()
            => _context.EventTickets.AsNoTracking()
                .Include(t => t.Event).ThenInclude(e => e.Branch).ThenInclude(b => b.Translations);
    }
}
