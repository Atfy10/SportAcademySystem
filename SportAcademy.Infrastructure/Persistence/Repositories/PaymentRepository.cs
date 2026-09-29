using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Application.DTOs.PaymentDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class PaymentRepository : BaseRepository<Payment, string>, IPaymentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentLanguageProvider _languageProvider;

        public PaymentRepository(ApplicationDbContext context, ICurrentLanguageProvider languageProvider)
            : base(context, languageProvider: languageProvider)
        {
            _context = context;
            _languageProvider = languageProvider;
        }

        // A trainee's payment history is now every Payment allocated against an invoice line
        // billing one of their subscriptions - one payment can appear once per subscription it
        // helped settle, which is the correct behavior once a payment can cover more than one
        // invoice.
        public async Task<List<PaymentHistoryDto>> GetHistoryForTraineeAsync(int traineeId, CancellationToken cancellationToken = default)
        {
            var rows = await _context.InvoiceLines
                .AsNoTracking()
                // Fee line only: a discounted subscription's invoice also carries a Discount line
                // for the same subscription, which listed every payment twice.
                .Where(l => l.SubscriptionDetails != null && l.SubscriptionDetails.TraineeId == traineeId
                    && l.Type == InvoiceLineType.SubscriptionFee)
                .SelectMany(l => l.Invoice.Allocations, (l, a) => new { Line = l, Allocation = a })
                .OrderByDescending(x => x.Allocation.Payment.PaidDate)
                .Select(x => new
                {
                    x.Allocation.Payment.PaymentNumber,
                    x.Allocation.Payment.PaymentTypeId,
                    PaymentTypeName = x.Allocation.Payment.PaymentType.Translations
                        .Where(t => t.LangCode == _languageProvider.Language).Select(t => t.Name).FirstOrDefault()
                        ?? x.Allocation.Payment.PaymentType.Name,
                    x.Allocation.Payment.PaidDate,
                    BranchName = x.Allocation.Payment.Branch.Translations
                        .Where(t => t.LangCode == _languageProvider.Language).Select(t => t.Name).FirstOrDefault()
                        ?? x.Allocation.Payment.Branch.Name,
                    SubscriptionDetailsId = x.Line.SubscriptionDetailsId!.Value,
                    SubscriptionTypeName = x.Line.SubscriptionDetails!.SportPrice.SportSubscriptionType.SubscriptionType.Name,
                    SportName = x.Line.SubscriptionDetails!.SportPrice.SportSubscriptionType.Sport.Translations
                        .Where(t => t.LangCode == _languageProvider.Language).Select(t => t.Name).FirstOrDefault()
                        ?? x.Line.SubscriptionDetails!.SportPrice.SportSubscriptionType.Sport.Name,
                    Price = x.Allocation.Amount,
                    x.Line.SubscriptionDetails!.StartDate,
                    x.Line.SubscriptionDetails!.EndDate,
                    Refunded = x.Allocation.ReversedAmount,
                    x.Allocation.Payment.Status,
                })
                .ToListAsync(cancellationToken);

            return rows.Select(r => new PaymentHistoryDto(
                r.PaymentNumber,
                r.PaymentTypeId,
                r.PaymentTypeName,
                r.PaidDate,
                r.BranchName,
                r.SubscriptionDetailsId,
                r.SubscriptionTypeName.ToString(),
                r.SportName,
                r.Price,
                r.StartDate,
                r.EndDate,
                r.Refunded,
                r.Status
            )).ToList();
        }

        public async Task<Payment?> GetWithAllocationsAsync(string paymentNumber, CancellationToken ct = default)
            => await _context.Payments
                .Include(p => p.Branch)
                .Include(p => p.PaymentType)
                .Include(p => p.Allocations)
                    .ThenInclude(a => a.Invoice)
                .SingleOrDefaultAsync(p => p.PaymentNumber == paymentNumber, ct);

        public async Task<Payment?> GetForReceiptAsync(string paymentNumber, CancellationToken ct = default)
            => await _context.Payments
                .AsNoTracking()
                .AsSplitQuery()
                .Include(p => p.Branch).ThenInclude(b => b.Translations)
                .Include(p => p.PaymentType).ThenInclude(pt => pt.Translations)
                .Include(p => p.Refunds)
                .Include(p => p.Allocations).ThenInclude(a => a.Invoice).ThenInclude(i => i.Trainee)
                .Include(p => p.Allocations).ThenInclude(a => a.Invoice).ThenInclude(i => i.Lines)
                    .ThenInclude(l => l.SubscriptionDetails!).ThenInclude(sd => sd.SportPrice)
                        .ThenInclude(sp => sp.SportSubscriptionType).ThenInclude(sst => sst.Sport).ThenInclude(s => s.Translations)
                .Include(p => p.Allocations).ThenInclude(a => a.Invoice).ThenInclude(i => i.Lines)
                    .ThenInclude(l => l.SubscriptionDetails!).ThenInclude(sd => sd.SportPrice)
                        .ThenInclude(sp => sp.SportSubscriptionType).ThenInclude(sst => sst.SubscriptionType)
                .SingleOrDefaultAsync(p => p.PaymentNumber == paymentNumber, ct);

        public async Task<(List<PaymentDto> Items, int TotalCount)> GetPagedAsync(
            PageRequest page, int? branchId, int? paymentTypeId, string? status,
            DateTime? from, DateTime? to, string? term = null, CancellationToken ct = default)
        {
            IQueryable<Payment> query = _context.Payments.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(term))
            {
                var t = term.Trim();
                query = query.Where(p =>
                    p.PaymentNumber.Contains(t)
                    || (p.Reference != null && p.Reference.Contains(t))
                    || p.Allocations.Any(a => a.Invoice.Trainee != null
                        && (a.Invoice.Trainee.FirstName.Contains(t)
                            || a.Invoice.Trainee.LastName.Contains(t)
                            || (a.Invoice.Trainee.FirstName + " " + a.Invoice.Trainee.LastName).Contains(t)
                            || a.Invoice.Trainee.PhoneNumber.Contains(t))));
            }

            if (branchId.HasValue)
                query = query.Where(p => p.BranchId == branchId.Value);

            if (paymentTypeId.HasValue)
                query = query.Where(p => p.PaymentTypeId == paymentTypeId.Value);

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentStatus>(status, true, out var parsedStatus))
                query = query.Where(p => p.Status == parsedStatus);

            if (from.HasValue)
                query = query.Where(p => p.PaidDate >= from.Value);

            if (to.HasValue)
            {
                var toExclusive = ReportDateRange.EndExclusive(to.Value);
                query = query.Where(p => p.PaidDate < toExclusive);
            }

            var lang = _languageProvider.Language;
            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(p => p.PaidDate)
                .Skip(page.Skip)
                .Take(page.PageSize)
                .Select(p => new PaymentDto(
                    p.PaymentNumber,
                    p.Amount,
                    p.RefundedAmount,
                    p.PaymentType.Translations.Where(x => x.LangCode == lang).Select(x => x.Name).FirstOrDefault() ?? p.PaymentType.Name,
                    p.Status,
                    p.PaidDate,
                    p.Branch.Translations.Where(x => x.LangCode == lang).Select(x => x.Name).FirstOrDefault() ?? p.Branch.Name,
                    p.Currency,
                    p.Reference,
                    p.Notes,
                    p.Allocations.OrderBy(a => a.Id).Select(a => a.Invoice.TraineeId).FirstOrDefault(),
                    p.Allocations.OrderBy(a => a.Id)
                        .Where(a => a.Invoice.Trainee != null)
                        .Select(a => a.Invoice.Trainee!.FirstName + " " + a.Invoice.Trainee.LastName)
                        .FirstOrDefault(),
                    p.Allocations.Where(a => a.Invoice.TraineeId != null).Select(a => a.Invoice.TraineeId).Distinct().Count(),
                    p.Allocations.OrderBy(a => a.Id).Select(a => a.Invoice.InvoiceNumber).ToList()))
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<List<(string GroupKey, decimal Gross, decimal Refunded, int Count)>> GetRevenueByMonthAsync(
            DateTime? from, DateTime? to, int? branchId, CancellationToken ct = default)
        {
            // Money in is placed in the month it was received; money back out (refunds and voids)
            // in the month it actually left, from the refund history - not netted against the
            // original payment's month, which would silently rewrite an already-reported month.
            var received = await FilteredPayments(from, to, branchId)
                .GroupBy(p => new { p.PaidDate.Year, p.PaidDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Gross = g.Sum(p => p.Amount), Count = g.Count() })
                .ToListAsync(ct);

            var refunded = await FilteredRefunds(from, to, branchId)
                .GroupBy(r => new { r.RefundedAt.Year, r.RefundedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Amount = g.Sum(r => r.Amount) })
                .ToListAsync(ct);

            return received.Select(r => (r.Year, r.Month)).Union(refunded.Select(r => (r.Year, r.Month)))
                .OrderBy(k => k.Year).ThenBy(k => k.Month)
                .Select(k =>
                {
                    var inRow = received.FirstOrDefault(r => r.Year == k.Year && r.Month == k.Month);
                    var outRow = refunded.FirstOrDefault(r => r.Year == k.Year && r.Month == k.Month);
                    return ($"{k.Year:D4}-{k.Month:D2}", inRow?.Gross ?? 0m, outRow?.Amount ?? 0m, inRow?.Count ?? 0);
                })
                .ToList();
        }

        public async Task<List<(string GroupKey, decimal Gross, decimal Refunded, int Count)>> GetRevenueByBranchAsync(
            DateTime? from, DateTime? to, int? branchId, CancellationToken ct = default)
        {
            // Group by Id, not Branch.Name - a raw-name group key can't be resolved to a
            // translated name inside the same GroupBy/Select (EF can't splice a per-request lang
            // into that projection), so the name lookup happens as a second, small query below.
            var received = await FilteredPayments(from, to, branchId)
                .GroupBy(p => p.BranchId)
                .Select(g => new { BranchId = g.Key, Gross = g.Sum(p => p.Amount), Count = g.Count() })
                .ToListAsync(ct);

            // Refunds dated inside the range, attributed to the branch that took the payment.
            var refundedByBranch = await FilteredRefunds(from, to, branchId)
                .GroupBy(r => r.Payment.BranchId)
                .Select(g => new { BranchId = g.Key, Amount = g.Sum(r => r.Amount) })
                .ToDictionaryAsync(x => x.BranchId, x => x.Amount, ct);

            var rows = received
                .Select(r => new { r.BranchId, r.Gross, Refunded = refundedByBranch.GetValueOrDefault(r.BranchId), r.Count })
                .Concat(refundedByBranch.Keys.Except(received.Select(r => r.BranchId))
                    .Select(id => new { BranchId = id, Gross = 0m, Refunded = refundedByBranch[id], Count = 0 }))
                .OrderByDescending(g => g.Gross)
                .ToList();

            if (rows.Count == 0) return [];

            var branchIds = rows.Select(r => r.BranchId).ToList();
            var branchNames = await _context.Branchs
                .Where(b => branchIds.Contains(b.Id))
                .Select(b => new
                {
                    b.Id,
                    Name = b.Translations.Where(t => t.LangCode == _languageProvider.Language).Select(t => t.Name).FirstOrDefault() ?? b.Name,
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

            return rows.Select(r => (branchNames.GetValueOrDefault(r.BranchId, string.Empty), r.Gross, r.Refunded, r.Count)).ToList();
        }

        public async Task<List<(string PaymentTypeName, decimal Total, int Count)>> GetPaymentMethodBreakdownAsync(
            DateTime? from, DateTime? to, int? branchId, CancellationToken ct = default)
        {
            // Net of refunds and voids: a voided payment was recorded in error and contributes
            // nothing (and isn't counted); a refunded one only what the academy kept.
            var rows = await FilteredPayments(from, to, branchId)
                .Where(p => p.Status != PaymentStatus.Voided)
                .GroupBy(p => p.PaymentTypeId)
                .Select(g => new { PaymentTypeId = g.Key, Total = g.Sum(p => p.Amount - p.RefundedAmount), Count = g.Count() })
                .OrderByDescending(g => g.Total)
                .ToListAsync(ct);

            if (rows.Count == 0) return [];

            var paymentTypeIds = rows.Select(r => r.PaymentTypeId).ToList();
            var paymentTypeNames = await _context.PaymentTypes
                .Where(pt => paymentTypeIds.Contains(pt.Id))
                .Select(pt => new
                {
                    pt.Id,
                    Name = pt.Translations.Where(t => t.LangCode == _languageProvider.Language).Select(t => t.Name).FirstOrDefault() ?? pt.Name,
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

            return rows.Select(r => (paymentTypeNames.GetValueOrDefault(r.PaymentTypeId, string.Empty), r.Total, r.Count)).ToList();
        }

        private IQueryable<Payment> FilteredPayments(DateTime? from, DateTime? to, int? branchId)
        {
            IQueryable<Payment> query = _context.Payments.AsNoTracking();

            if (from.HasValue) query = query.Where(p => p.PaidDate >= from.Value);
            if (to.HasValue)
            {
                var toExclusive = ReportDateRange.EndExclusive(to.Value);
                query = query.Where(p => p.PaidDate < toExclusive);
            }
            if (branchId.HasValue) query = query.Where(p => p.BranchId == branchId.Value);

            return query;
        }

        private IQueryable<PaymentRefund> FilteredRefunds(DateTime? from, DateTime? to, int? branchId)
        {
            IQueryable<PaymentRefund> query = _context.PaymentRefunds.AsNoTracking();

            if (from.HasValue) query = query.Where(r => r.RefundedAt >= from.Value);
            if (to.HasValue)
            {
                var toExclusive = ReportDateRange.EndExclusive(to.Value);
                query = query.Where(r => r.RefundedAt < toExclusive);
            }
            if (branchId.HasValue) query = query.Where(r => r.Payment.BranchId == branchId.Value);

            return query;
        }
    }
}
