using SportAcademy.Application.Common.Pagination;
using SportAcademy.Domain.Entities.Finance;

namespace SportAcademy.Application.Interfaces
{
    public interface IExpenseRepository : IBaseRepository<Expense, int>
    {
        Task<(List<Expense> Items, int TotalCount)> GetPagedAsync(
            PageRequest page, int? branchId, int? expenseCategoryId, DateOnly? from, DateOnly? to,
            CancellationToken cancellationToken = default);

        Task<Expense?> GetByIdWithIncludesAsync(int id, CancellationToken cancellationToken = default);

        // Grouped by ExpenseDate's calendar month, in this tenant's own timezone-naive DateOnly -
        // same "yyyy-MM" GroupKey format IPaymentRepository.GetRevenueByMonthAsync uses, so
        // GetFinancialReportQueryHandler can merge both series on a matching key.
        Task<List<(string GroupKey, decimal Total)>> GetExpensesByMonthAsync(
            DateTime? from, DateTime? to, int? branchId, CancellationToken cancellationToken = default);
    }
}
