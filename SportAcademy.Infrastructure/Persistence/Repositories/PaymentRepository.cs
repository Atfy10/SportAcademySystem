using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.PaymentDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class PaymentRepository : BaseRepository<Payment, string>, IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        // A trainee's payment history is now every Payment allocated against an invoice line
        // billing one of their subscriptions - one payment can appear once per subscription it
        // helped settle, which is the correct behavior once a payment can cover more than one
        // invoice.
        public async Task<List<PaymentHistoryDto>> GetHistoryForTraineeAsync(int traineeId, CancellationToken cancellationToken = default)
        {
            var rows = await _context.InvoiceLines
                .AsNoTracking()
                .Where(l => l.SubscriptionDetails != null && l.SubscriptionDetails.TraineeId == traineeId)
                .SelectMany(l => l.Invoice.Allocations, (l, a) => new { Line = l, Allocation = a })
                .OrderByDescending(x => x.Allocation.Payment.PaidDate)
                .Select(x => new
                {
                    x.Allocation.Payment.PaymentNumber,
                    PaymentTypeName = x.Allocation.Payment.PaymentType.Name,
                    x.Allocation.Payment.PaidDate,
                    BranchName = x.Allocation.Payment.Branch.Name,
                    SubscriptionDetailsId = x.Line.SubscriptionDetailsId!.Value,
                    SubscriptionTypeName = x.Line.SubscriptionDetails!.SportPrice.SportSubscriptionType.SubscriptionType.Name,
                    SportName = x.Line.SubscriptionDetails!.SportPrice.SportSubscriptionType.Sport.Name,
                    Price = x.Allocation.Amount,
                    x.Line.SubscriptionDetails!.StartDate,
                    x.Line.SubscriptionDetails!.EndDate,
                })
                .ToListAsync(cancellationToken);

            return rows.Select(r => new PaymentHistoryDto(
                r.PaymentNumber,
                r.PaymentTypeName,
                r.PaidDate,
                r.BranchName,
                r.SubscriptionDetailsId,
                r.SubscriptionTypeName.ToString(),
                r.SportName,
                r.Price,
                r.StartDate,
                r.EndDate
            )).ToList();
        }

        public async Task<Payment?> GetWithAllocationsAsync(string paymentNumber, CancellationToken ct = default)
            => await _context.Payments
                .Include(p => p.Branch)
                .Include(p => p.PaymentType)
                .Include(p => p.Allocations)
                    .ThenInclude(a => a.Invoice)
                .SingleOrDefaultAsync(p => p.PaymentNumber == paymentNumber, ct);

        public async Task<(List<Payment> Items, int TotalCount)> GetPagedAsync(
            PageRequest page, int? branchId, int? paymentTypeId, string? status,
            DateTime? from, DateTime? to, CancellationToken ct = default)
        {
            IQueryable<Payment> query = _context.Payments
                .Include(p => p.Branch)
                .Include(p => p.PaymentType)
                .AsNoTracking();

            if (branchId.HasValue)
                query = query.Where(p => p.BranchId == branchId.Value);

            if (paymentTypeId.HasValue)
                query = query.Where(p => p.PaymentTypeId == paymentTypeId.Value);

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentStatus>(status, true, out var parsedStatus))
                query = query.Where(p => p.Status == parsedStatus);

            if (from.HasValue)
                query = query.Where(p => p.PaidDate >= from.Value);

            if (to.HasValue)
                query = query.Where(p => p.PaidDate <= to.Value);

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(p => p.PaidDate)
                .Skip(page.Skip)
                .Take(page.PageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<List<(string GroupKey, decimal Gross, decimal Refunded, int Count)>> GetRevenueByMonthAsync(
            DateTime? from, DateTime? to, int? branchId, CancellationToken ct = default)
        {
            var query = FilteredPayments(from, to, branchId);

            var rows = await query
                .GroupBy(p => new { p.PaidDate.Year, p.PaidDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Gross = g.Sum(p => p.Amount),
                    Refunded = g.Sum(p => p.RefundedAmount),
                    Count = g.Count(),
                })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync(ct);

            return rows.Select(r => ($"{r.Year:D4}-{r.Month:D2}", r.Gross, r.Refunded, r.Count)).ToList();
        }

        public async Task<List<(string GroupKey, decimal Gross, decimal Refunded, int Count)>> GetRevenueByBranchAsync(
            DateTime? from, DateTime? to, int? branchId, CancellationToken ct = default)
        {
            var query = FilteredPayments(from, to, branchId);

            var rows = await query
                .GroupBy(p => p.Branch.Name)
                .Select(g => new
                {
                    BranchName = g.Key,
                    Gross = g.Sum(p => p.Amount),
                    Refunded = g.Sum(p => p.RefundedAmount),
                    Count = g.Count(),
                })
                .OrderByDescending(g => g.Gross)
                .ToListAsync(ct);

            return rows.Select(r => (r.BranchName, r.Gross, r.Refunded, r.Count)).ToList();
        }

        public async Task<List<(string PaymentTypeName, decimal Total, int Count)>> GetPaymentMethodBreakdownAsync(
            DateTime? from, DateTime? to, int? branchId, CancellationToken ct = default)
        {
            var query = FilteredPayments(from, to, branchId);

            var rows = await query
                .GroupBy(p => p.PaymentType.Name)
                .Select(g => new { PaymentTypeName = g.Key, Total = g.Sum(p => p.Amount), Count = g.Count() })
                .OrderByDescending(g => g.Total)
                .ToListAsync(ct);

            return rows.Select(r => (r.PaymentTypeName, r.Total, r.Count)).ToList();
        }

        private IQueryable<Payment> FilteredPayments(DateTime? from, DateTime? to, int? branchId)
        {
            IQueryable<Payment> query = _context.Payments.AsNoTracking();

            if (from.HasValue) query = query.Where(p => p.PaidDate >= from.Value);
            if (to.HasValue) query = query.Where(p => p.PaidDate <= to.Value);
            if (branchId.HasValue) query = query.Where(p => p.BranchId == branchId.Value);

            return query;
        }
    }
}
