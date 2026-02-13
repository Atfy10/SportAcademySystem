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
        Task<Employee?> GetFullEmployee(int id, CancellationToken cancellationToken = default);
        Task<List<Employee>> GetActiveAsync(CancellationToken cancellationToken = default);
        Task<List<Employee>> GetActiveCoachesAsync(CancellationToken cancellationToken = default);

    }
}
