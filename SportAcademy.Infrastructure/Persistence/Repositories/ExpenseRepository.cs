using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class ExpenseRepository : BaseRepository<Expense, int>, IExpenseRepository
    {
        private readonly ApplicationDbContext _context;

        public ExpenseRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<(List<Expense> Items, int TotalCount)> GetPagedAsync(
            PageRequest page, int? branchId, int? expenseCategoryId, DateOnly? from, DateOnly? to,
            CancellationToken cancellationToken = default)
        {
            IQueryable<Expense> query = _context.Expenses
                .Include(e => e.ExpenseCategory)
                .Include(e => e.Branch)
                .Include(e => e.PaymentType)
                .AsNoTracking();

            if (branchId.HasValue)
                query = query.Where(e => e.BranchId == branchId.Value);

            if (expenseCategoryId.HasValue)
                query = query.Where(e => e.ExpenseCategoryId == expenseCategoryId.Value);

            if (from.HasValue)
                query = query.Where(e => e.ExpenseDate >= from.Value);

            if (to.HasValue)
                query = query.Where(e => e.ExpenseDate <= to.Value);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(e => e.ExpenseDate)
                .Skip(page.Skip)
                .Take(page.PageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<Expense?> GetByIdWithIncludesAsync(int id, CancellationToken cancellationToken = default)
            => await _context.Expenses
                .Include(e => e.ExpenseCategory)
                .Include(e => e.Branch)
                .Include(e => e.PaymentType)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        public async Task<List<(string GroupKey, decimal Total)>> GetExpensesByMonthAsync(
            DateTime? from, DateTime? to, int? branchId, CancellationToken cancellationToken = default)
        {
            IQueryable<Expense> query = _context.Expenses.AsNoTracking();

            if (from.HasValue) query = query.Where(e => e.ExpenseDate >= DateOnly.FromDateTime(from.Value));
            if (to.HasValue) query = query.Where(e => e.ExpenseDate <= DateOnly.FromDateTime(to.Value));
            if (branchId.HasValue) query = query.Where(e => e.BranchId == branchId.Value);

            var rows = await query
                .GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(e => e.Amount) })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync(cancellationToken);

            return rows.Select(r => ($"{r.Year:D4}-{r.Month:D2}", r.Total)).ToList();
        }
    }
}
