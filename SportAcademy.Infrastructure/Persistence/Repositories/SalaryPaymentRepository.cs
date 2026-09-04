using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class SalaryPaymentRepository : BaseRepository<SalaryPayment, int>, ISalaryPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public SalaryPaymentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<(List<SalaryPayment> Items, int TotalCount)> GetPagedAsync(
            PageRequest page, int? employeeId, int? branchId, SalaryPaymentStatus? status,
            DateOnly? periodFrom, DateOnly? periodTo, CancellationToken cancellationToken = default)
        {
            IQueryable<SalaryPayment> query = _context.SalaryPayments
                .Include(sp => sp.Employee)
                .Include(sp => sp.Branch)
                .Include(sp => sp.PaymentType)
                .AsNoTracking();

            if (employeeId.HasValue)
                query = query.Where(sp => sp.EmployeeId == employeeId.Value);

            if (branchId.HasValue)
                query = query.Where(sp => sp.BranchId == branchId.Value);

            if (status.HasValue)
                query = query.Where(sp => sp.Status == status.Value);

            if (periodFrom.HasValue)
                query = query.Where(sp => sp.PeriodMonth >= periodFrom.Value);

            if (periodTo.HasValue)
                query = query.Where(sp => sp.PeriodMonth <= periodTo.Value);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(sp => sp.PeriodMonth)
                .Skip(page.Skip)
                .Take(page.PageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<SalaryPayment?> GetByIdWithIncludesAsync(int id, CancellationToken cancellationToken = default)
            => await _context.SalaryPayments
                .Include(sp => sp.Employee)
                .Include(sp => sp.Branch)
                .Include(sp => sp.PaymentType)
                .AsNoTracking()
                .FirstOrDefaultAsync(sp => sp.Id == id, cancellationToken);

        public async Task<List<PayrollEmployeeDto>> GetPayrollEmployeesAsync(
            int? branchId, string? search, CancellationToken cancellationToken = default)
        {
            var query = ApplyBranchFilter(_context.Employees)
                .Include(e => e.Branch)
                .Include(e => e.Coach)
                .AsNoTracking()
                .Where(e => e.IsWork);

            if (branchId.HasValue)
                query = query.Where(e => e.BranchId == branchId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(e => e.FirstName.Contains(term) || e.LastName.Contains(term));
            }

            var rows = await query
                .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
                .Select(e => new
                {
                    e.Id,
                    e.FirstName,
                    e.LastName,
                    e.BranchId,
                    BranchName = e.Branch.Name,
                    e.Position,
                    e.Salary,
                    IsCoach = e.Coach != null,
                    CoachEmployeeId = e.Coach != null ? (int?)e.Coach.EmployeeId : null,
                })
                .ToListAsync(cancellationToken);

            return rows.Select(r => new PayrollEmployeeDto(
                r.Id,
                $"{r.FirstName} {r.LastName}",
                r.BranchId,
                r.BranchName,
                r.Position.ToString(),
                r.Salary,
                r.IsCoach,
                r.CoachEmployeeId
            )).ToList();
        }

        public async Task<List<(string GroupKey, decimal Total)>> GetPaidSalariesByMonthAsync(
            DateTime? from, DateTime? to, int? branchId, CancellationToken cancellationToken = default)
        {
            IQueryable<SalaryPayment> query = _context.SalaryPayments
                .AsNoTracking()
                .Where(sp => sp.Status == SalaryPaymentStatus.Paid && sp.PaidAt != null);

            if (from.HasValue) query = query.Where(sp => sp.PaidAt >= from.Value);
            if (to.HasValue) query = query.Where(sp => sp.PaidAt <= to.Value);
            if (branchId.HasValue) query = query.Where(sp => sp.BranchId == branchId.Value);

            var rows = await query
                .GroupBy(sp => new { sp.PaidAt!.Value.Year, sp.PaidAt!.Value.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(sp => sp.Amount) })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync(cancellationToken);

            return rows.Select(r => ($"{r.Year:D4}-{r.Month:D2}", r.Total)).ToList();
        }
    }
}
