using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface IEmployeeRepository : IBaseRepository<Employee, int>, IPersonRepository
    {
        Task<int> GetActiveEmployeesCountAsync(CancellationToken ct = default);
        Task<int> GetActiveCoachesCountAsync(CancellationToken ct = default);
        Task<PagedData<EmployeeDto>> GetAllAsync(PageRequest page, CancellationToken cancellationToken = default);
        Task<Employee?> GetFullEmployee(int id, CancellationToken cancellationToken = default);
        Task<PagedData<EmployeeDto>> GetActiveAsync(PageRequest page, CancellationToken cancellationToken = default);
        Task<PagedData<EmployeeDto>> GetActiveCoachesAsync(PageRequest page, CancellationToken cancellationToken = default);
        Task<PagedData<EmployeeDto>> GetCoachEmployeesWithoutCoachRecordAsync(PageRequest page, CancellationToken cancellationToken = default);
    }
}
