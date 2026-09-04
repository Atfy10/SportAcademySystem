using SportAcademy.Domain.Entities.Finance;

namespace SportAcademy.Application.Interfaces
{
    public interface IExpenseCategoryRepository : IBaseRepository<ExpenseCategory, int>
    {
        Task<bool> AnyExpensesInCategoryAsync(int expenseCategoryId, CancellationToken cancellationToken = default);
    }
}
