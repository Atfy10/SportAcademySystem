using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Interfaces
{
    public interface ISalaryPaymentRepository : IBaseRepository<SalaryPayment, int>
    {
        Task<(List<SalaryPayment> Items, int TotalCount)> GetPagedAsync(
            PageRequest page, int? employeeId, int? branchId, SalaryPaymentStatus? status,
            DateOnly? periodFrom, DateOnly? periodTo, CancellationToken cancellationToken = default);

        Task<SalaryPayment?> GetByIdWithIncludesAsync(int id, CancellationToken cancellationToken = default);

        // Every active Employee left-joined to their Coach record - IsCoach/CoachId are null
        // when the employee has none. Lives here (not IEmployeeRepository) since it's a
        // payroll-roster view, not a generic employee listing.
        Task<List<PayrollEmployeeDto>> GetPayrollEmployeesAsync(
            int? branchId, string? search, CancellationToken cancellationToken = default);

        // Grouped by PaidAt's calendar month (what actually left the bank that month), not
        // PeriodMonth - only Status == Paid rows count. Same "yyyy-MM" GroupKey format as
        // IPaymentRepository.GetRevenueByMonthAsync / IExpenseRepository.GetExpensesByMonthAsync.
        Task<List<(string GroupKey, decimal Total)>> GetPaidSalariesByMonthAsync(
            DateTime? from, DateTime? to, int? branchId, CancellationToken cancellationToken = default);
    }
}
