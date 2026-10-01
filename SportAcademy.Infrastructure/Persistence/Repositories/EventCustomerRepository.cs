using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Events;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Services;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class EventCustomerRepository : BaseRepository<EventCustomer, int>, IEventCustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public EventCustomerRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<EventCustomer?> GetByPhoneAsync(string phoneE164, CancellationToken ct = default)
            => await _context.EventCustomers.FirstOrDefaultAsync(c => c.PhoneNumber == phoneE164, ct);

        public async Task<bool> IsPhoneTakenAsync(string phoneE164, int? excludeId, CancellationToken ct = default)
            => await _context.EventCustomers.AnyAsync(
                c => c.PhoneNumber == phoneE164 && (excludeId == null || c.Id != excludeId), ct);

        // Every non-deleted event counts, cancelled included (it's still on record) - and
        // regardless of the caller's branch access: a branch-restricted user who can't see an
        // event at another branch must still not be able to delete the customer out from under it.
        // IgnoreQueryFilters drops the tenant filter too, so it's re-applied by hand.
        public async Task<bool> HasEventsAsync(int customerId, CancellationToken ct = default)
        {
            var tenantId = _context.CurrentTenantId;
            return await _context.Events.IgnoreQueryFilters()
                .AnyAsync(e => e.EventCustomerId == customerId && !e.IsDeleted && e.TenantId == tenantId, ct);
        }

        public async Task<(List<EventCustomerDto> Items, int TotalCount)> GetPagedAsync(
            PageRequest page, string? term, int? nationalityCategoryId, bool? isActive, string lang,
            CancellationToken ct = default)
        {
            IQueryable<EventCustomer> query = _context.EventCustomers.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(term))
            {
                var t = term.Trim();
                query = query.Where(c => c.FullName.Contains(t) || c.PhoneNumber.Contains(t));
            }

            if (nationalityCategoryId is { } categoryId)
                query = query.Where(c => c.NationalityCategoryId == categoryId);

            if (isActive is { } active)
                query = query.Where(c => c.IsActive == active);

            var totalCount = await query.CountAsync(ct);
            var items = await Project(query.OrderBy(c => c.FullName).ThenBy(c => c.Id), lang)
                .Skip(page.Skip)
                .Take(page.PageSize)
                .ToListAsync(ct);

            return (items.Select(ToAcademyTime).ToList(), totalCount);
        }

        public async Task<EventCustomerDto?> GetSummaryAsync(int id, string lang, CancellationToken ct = default)
        {
            var dto = await Project(_context.EventCustomers.AsNoTracking().Where(c => c.Id == id), lang)
                .SingleOrDefaultAsync(ct);
            return dto is null ? null : ToAcademyTime(dto);
        }

        // LastEventAt is computed from the stored UTC start; shown as the academy's wall clock.
        private static EventCustomerDto ToAcademyTime(EventCustomerDto dto)
            => dto.LastEventAt is { } utc ? dto with { LastEventAt = TenantCalendar.ToLocal(utc) } : dto;

        // c.Events goes through the Event query filters (tenant, soft delete, branch access), so
        // the numbers only cover events the caller is allowed to see.
        private static IQueryable<EventCustomerDto> Project(IQueryable<EventCustomer> query, string lang)
            => query.Select(c => new EventCustomerDto(
                c.Id,
                c.FullName,
                c.PhoneNumber,
                c.NationalityCategoryId,
                c.NationalityCategory.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault()
                    ?? c.NationalityCategory.Name,
                c.Notes,
                c.IsActive,
                c.Events.Count(e => !e.IsCancelled),
                c.Events.Where(e => !e.IsCancelled).Max(e => (DateTime?)e.StartsAt),
                c.Events.Where(e => e.Invoice != null && e.Invoice.Status != InvoiceStatus.Cancelled)
                    .Sum(e => (decimal?)e.Invoice!.GrandTotal) ?? 0m,
                c.Events.Where(e => e.Invoice != null && e.Invoice.Status != InvoiceStatus.Cancelled)
                    .Sum(e => (decimal?)e.Invoice!.AmountPaid) ?? 0m,
                c.Events.Where(e => e.Invoice != null && e.Invoice.Status != InvoiceStatus.Cancelled)
                    .Sum(e => (decimal?)(e.Invoice!.GrandTotal - e.Invoice.AmountPaid)) ?? 0m,
                c.CreatedAt));
    }
}
