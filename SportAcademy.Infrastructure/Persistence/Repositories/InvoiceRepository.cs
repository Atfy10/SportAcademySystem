using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class InvoiceRepository : BaseRepository<Invoice, int>, IInvoiceRepository
    {
        private readonly ApplicationDbContext _context;

        public InvoiceRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Invoice?> GetWithLinesAndAllocationsAsync(int invoiceId, CancellationToken ct = default)
            => await _context.Invoices
                .Include(i => i.Trainee)
                .Include(i => i.Branch)
                .Include(i => i.Lines)
                .Include(i => i.Allocations)
                    .ThenInclude(a => a.Payment)
                .SingleOrDefaultAsync(i => i.Id == invoiceId, ct);

        public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken ct = default)
            => await _context.Invoices
                .Include(i => i.Lines)
                .SingleOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber, ct);

        public async Task<Invoice?> GetBySubscriptionDetailsIdAsync(int subscriptionDetailsId, CancellationToken ct = default)
            => await _context.Invoices
                .Include(i => i.Lines)
                .FirstOrDefaultAsync(i => i.Lines.Any(l => l.SubscriptionDetailsId == subscriptionDetailsId), ct);

        public async Task<List<Invoice>> GetByIdsWithLinesAsync(IEnumerable<int> ids, CancellationToken ct = default)
            => await _context.Invoices
                .Include(i => i.Lines)
                .Where(i => ids.Contains(i.Id))
                .ToListAsync(ct);

        public async Task<(List<Invoice> Items, int TotalCount)> GetPagedAsync(
            PageRequest page, int? branchId, string? status, CancellationToken ct = default)
        {
            IQueryable<Invoice> query = _context.Invoices
                .Include(i => i.Trainee)
                .Include(i => i.Branch)
                .Include(i => i.Lines)
                .AsNoTracking();

            if (branchId.HasValue)
                query = query.Where(i => i.BranchId == branchId.Value);

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<InvoiceStatus>(status, true, out var parsedStatus))
                query = query.Where(i => i.Status == parsedStatus);

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(i => i.Id)
                .Skip(page.Skip)
                .Take(page.PageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<(List<Invoice> Items, int TotalCount)> GetOutstandingAsync(
            PageRequest page, int? branchId, bool overdueOnly, CancellationToken ct = default)
        {
            IQueryable<Invoice> query = _context.Invoices
                .Include(i => i.Trainee)
                .Include(i => i.Branch)
                .AsNoTracking()
                .Where(i => i.Status != InvoiceStatus.Paid
                         && i.Status != InvoiceStatus.Cancelled
                         && i.AmountPaid < i.GrandTotal);

            if (branchId.HasValue)
                query = query.Where(i => i.BranchId == branchId.Value);

            if (overdueOnly)
                query = query.Where(i => i.DueDate < DateOnly.FromDateTime(DateTime.UtcNow));

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderBy(i => i.DueDate)
                .Skip(page.Skip)
                .Take(page.PageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<(decimal TotalOutstanding, int InvoiceCount, int OverdueCount, decimal OverdueAmount)> GetOutstandingSummaryAsync(
            int? branchId, CancellationToken ct = default)
        {
            IQueryable<Invoice> query = _context.Invoices
                .AsNoTracking()
                .Where(i => i.Status != InvoiceStatus.Paid
                         && i.Status != InvoiceStatus.Cancelled
                         && i.AmountPaid < i.GrandTotal);

            if (branchId.HasValue)
                query = query.Where(i => i.BranchId == branchId.Value);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var rows = await query
                .Select(i => new { Outstanding = i.GrandTotal - i.AmountPaid, IsOverdue = i.DueDate < today })
                .ToListAsync(ct);

            return (
                rows.Sum(r => r.Outstanding),
                rows.Count,
                rows.Count(r => r.IsOverdue),
                rows.Where(r => r.IsOverdue).Sum(r => r.Outstanding)
            );
        }
    }
}
