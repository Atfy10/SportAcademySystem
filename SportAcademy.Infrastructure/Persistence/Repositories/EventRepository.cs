using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Events;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Services;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class EventRepository : BaseRepository<Event, int>, IEventRepository
    {
        private readonly ApplicationDbContext _context;

        public EventRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<(List<Event> Items, int TotalCount)> GetPagedAsync(
            EventListFilter filter, DateOnly today, PageRequest page, CancellationToken ct = default)
        {
            var query = Filtered(filter, today);

            var totalCount = await query.CountAsync(ct);
            var items = await WithListIncludes(query)
                .OrderByDescending(e => e.StartsAt)
                .ThenByDescending(e => e.Id)
                .Skip(page.Skip)
                .Take(page.PageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<List<Event>> GetForReportAsync(
            EventListFilter filter, DateOnly today, int maxRows, CancellationToken ct = default)
            => await WithListIncludes(Filtered(filter, today))
                .OrderBy(e => e.StartsAt)
                .ThenBy(e => e.Id)
                .Take(maxRows)
                .ToListAsync(ct);

        public async Task<Event?> GetWithDetailsAsync(int id, bool forUpdate, CancellationToken ct = default)
        {
            IQueryable<Event> query = _context.Events
                .AsSplitQuery()
                .Include(e => e.Branch).ThenInclude(b => b.Translations)
                .Include(e => e.EventCustomer).ThenInclude(c => c.NationalityCategory).ThenInclude(n => n.Translations)
                .Include(e => e.Invoice!).ThenInclude(i => i.Lines)
                .Include(e => e.Invoice!).ThenInclude(i => i.Allocations).ThenInclude(a => a.Payment)
                    .ThenInclude(p => p.PaymentType).ThenInclude(pt => pt.Translations);

            if (!forUpdate)
                query = query.AsNoTracking();

            return await query.SingleOrDefaultAsync(e => e.Id == id, ct);
        }

        public async Task<List<Event>> GetOverlappingAsync(
            int branchId, DateTime startsAt, DateTime endsAt, int? excludeId, CancellationToken ct = default)
            => await _context.Events
                .AsNoTracking()
                .Include(e => e.EventCustomer)
                .Where(e => e.BranchId == branchId
                         && !e.IsCancelled
                         && (excludeId == null || e.Id != excludeId)
                         && e.StartsAt < endsAt
                         && e.EndsAt > startsAt)
                .OrderBy(e => e.StartsAt)
                .ToListAsync(ct);

        private IQueryable<Event> Filtered(EventListFilter filter, DateOnly today)
        {
            IQueryable<Event> query = _context.Events.AsNoTracking();

            if (filter.BranchId is { } branchId)
                query = query.Where(e => e.BranchId == branchId);

            if (filter.CustomerId is { } customerId)
                query = query.Where(e => e.EventCustomerId == customerId);

            if (filter.CreatedByUserId is { } createdBy)
                query = query.Where(e => e.CreatedByUserId == createdBy);

            // Inclusive range of the academy's calendar days against the event's whole span.
            if (filter.From is { } from)
            {
                var fromStart = TenantCalendar.DayStartUtc(from);
                query = query.Where(e => e.EndsAt > fromStart);
            }

            if (filter.To is { } to)
            {
                var toExclusive = TenantCalendar.DayStartUtc(to.AddDays(1));
                query = query.Where(e => e.StartsAt < toExclusive);
            }

            if (filter.Status is { } status)
            {
                var (todayStart, tomorrowStart) = EventStatusRules.DayBounds(today);
                query = status switch
                {
                    EventStatus.Cancelled => query.Where(e => e.IsCancelled),
                    EventStatus.Upcoming => query.Where(e => !e.IsCancelled && e.StartsAt >= tomorrowStart),
                    EventStatus.Completed => query.Where(e => !e.IsCancelled && e.EndsAt < todayStart),
                    _ => query.Where(e => !e.IsCancelled && e.StartsAt < tomorrowStart && e.EndsAt >= todayStart),
                };
            }

            if (!string.IsNullOrWhiteSpace(filter.Term))
            {
                var t = filter.Term.Trim();
                query = query.Where(e =>
                    e.Title.Contains(t)
                    || e.EventCustomer.FullName.Contains(t)
                    || e.EventCustomer.PhoneNumber.Contains(t));
            }

            return query;
        }

        private static IQueryable<Event> WithListIncludes(IQueryable<Event> query)
            => query
                .AsSplitQuery()
                .Include(e => e.Branch).ThenInclude(b => b.Translations)
                .Include(e => e.EventCustomer).ThenInclude(c => c.NationalityCategory).ThenInclude(n => n.Translations)
                .Include(e => e.Invoice);
    }
}
