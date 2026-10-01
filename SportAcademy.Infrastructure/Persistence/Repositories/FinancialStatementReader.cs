using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class FinancialStatementReader : IFinancialStatementReader
    {
        private readonly ApplicationDbContext _context;

        public FinancialStatementReader(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<IncomeLineRow>> GetIncomeAsync(
            DateTime? from, DateTime? to, int? branchId, string lang, int limit, CancellationToken ct = default)
        {
            var query = _context.PaymentAllocations.AsNoTracking();
            if (from.HasValue) query = query.Where(a => a.Payment.PaidDate >= from.Value);
            if (to.HasValue)
            {
                var toExclusive = ReportDateRange.EndExclusive(to.Value);
                query = query.Where(a => a.Payment.PaidDate < toExclusive);
            }
            if (branchId.HasValue) query = query.Where(a => a.Payment.BranchId == branchId.Value);

            var rows = await query
                .OrderBy(a => a.Payment.PaidDate).ThenBy(a => a.Id)
                .Take(limit)
                .Select(a => new
                {
                    a.Payment.PaidDate,
                    a.PaymentNumber,
                    BranchName = a.Payment.Branch.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault()
                        ?? a.Payment.Branch.Name,
                    a.Amount,
                    IsEvent = a.Invoice.Lines.Any(l => l.Type == InvoiceLineType.EventFee),
                    IsSubscription = a.Invoice.Lines.Any(l => l.Type == InvoiceLineType.SubscriptionFee),
                    TraineeName = a.Invoice.Trainee != null ? a.Invoice.Trainee.FirstName + " " + a.Invoice.Trainee.LastName : null,
                    a.Invoice.PayerName,
                    SportName = a.Invoice.Lines
                        .Where(l => l.SubscriptionDetails != null)
                        .Select(l => l.SubscriptionDetails!.SportPrice.SportSubscriptionType.Sport.Translations
                                .Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault()
                            ?? l.SubscriptionDetails.SportPrice.SportSubscriptionType.Sport.Name)
                        .FirstOrDefault(),
                    PlanName = a.Invoice.Lines
                        .Where(l => l.SubscriptionDetails != null)
                        .Select(l => l.SubscriptionDetails!.SportPrice.SportSubscriptionType.SubscriptionType.Name)
                        .FirstOrDefault(),
                    EventTitle = a.Invoice.Lines
                        .Where(l => l.Event != null)
                        .Select(l => l.Event!.Title)
                        .FirstOrDefault(),
                })
                .ToListAsync(ct);

            return rows.Select(r => new IncomeLineRow(
                r.PaidDate, r.PaymentNumber, r.BranchName, r.Amount,
                r.IsEvent ? IncomeKind.Event : r.IsSubscription ? IncomeKind.Subscription : IncomeKind.Other,
                r.TraineeName, r.PayerName, r.SportName, r.PlanName, r.EventTitle)).ToList();
        }

        public async Task<List<RefundLineRow>> GetRefundsAsync(
            DateTime? from, DateTime? to, int? branchId, string lang, int limit, CancellationToken ct = default)
        {
            var query = _context.PaymentRefunds.AsNoTracking();
            if (from.HasValue) query = query.Where(r => r.RefundedAt >= from.Value);
            if (to.HasValue)
            {
                var toExclusive = ReportDateRange.EndExclusive(to.Value);
                query = query.Where(r => r.RefundedAt < toExclusive);
            }
            if (branchId.HasValue) query = query.Where(r => r.Payment.BranchId == branchId.Value);

            return await query
                .OrderBy(r => r.RefundedAt).ThenBy(r => r.Id)
                .Take(limit)
                .Select(r => new RefundLineRow(
                    r.RefundedAt,
                    r.PaymentNumber,
                    r.Kind,
                    r.Reason,
                    r.Amount,
                    r.Payment.Branch.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault()
                        ?? r.Payment.Branch.Name,
                    r.Payment.Allocations.OrderBy(a => a.Id)
                        .Select(a => a.Invoice.Trainee != null
                            ? a.Invoice.Trainee.FirstName + " " + a.Invoice.Trainee.LastName
                            : a.Invoice.PayerName)
                        .FirstOrDefault()))
                .ToListAsync(ct);
        }

        public async Task<List<ExpenseLineRow>> GetExpensesAsync(
            DateTime? from, DateTime? to, int? branchId, string lang, int limit, CancellationToken ct = default)
        {
            var query = _context.Expenses.AsNoTracking();
            if (from.HasValue) query = query.Where(e => e.ExpenseDate >= DateOnly.FromDateTime(from.Value));
            if (to.HasValue) query = query.Where(e => e.ExpenseDate <= DateOnly.FromDateTime(to.Value));
            if (branchId.HasValue) query = query.Where(e => e.BranchId == branchId.Value);

            return await query
                .OrderBy(e => e.ExpenseDate).ThenBy(e => e.Id)
                .Take(limit)
                .Select(e => new ExpenseLineRow(
                    e.ExpenseDate,
                    e.Title,
                    e.ExpenseCategoryId,
                    e.ExpenseCategory.Name,
                    e.Branch.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? e.Branch.Name,
                    e.Amount,
                    e.Notes))
                .ToListAsync(ct);
        }

        public async Task<List<SalaryLineRow>> GetPaidSalariesAsync(
            DateTime? from, DateTime? to, int? branchId, string lang, int limit, CancellationToken ct = default)
        {
            var query = _context.SalaryPayments.AsNoTracking()
                .Where(sp => sp.Status == SalaryPaymentStatus.Paid && sp.PaidAt != null);
            if (from.HasValue) query = query.Where(sp => sp.PaidAt >= from.Value);
            if (to.HasValue)
            {
                var toExclusive = ReportDateRange.EndExclusive(to.Value);
                query = query.Where(sp => sp.PaidAt < toExclusive);
            }
            if (branchId.HasValue) query = query.Where(sp => sp.BranchId == branchId.Value);

            return await query
                .OrderBy(sp => sp.PaidAt).ThenBy(sp => sp.Id)
                .Take(limit)
                .Select(sp => new SalaryLineRow(
                    sp.PaidAt!.Value,
                    sp.PeriodMonth,
                    sp.Employee.FirstName + " " + sp.Employee.LastName,
                    sp.Branch.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? sp.Branch.Name,
                    sp.Amount,
                    sp.Bonus))
                .ToListAsync(ct);
        }
    }
}
