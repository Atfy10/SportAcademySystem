using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class ExpenseCategoryRepository : BaseRepository<ExpenseCategory, int>, IExpenseCategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public ExpenseCategoryRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> AnyExpensesInCategoryAsync(int expenseCategoryId, CancellationToken cancellationToken = default)
            => await _context.Expenses.AnyAsync(e => e.ExpenseCategoryId == expenseCategoryId, cancellationToken);
    }
}
