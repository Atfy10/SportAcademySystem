using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface IEmployeeRepository : IBaseRepository<Employee, int>
    {
        Task<bool> IsSSNExistAsync(string ssn, CancellationToken cancellationToken = default);
        Task<Trainee?> GetFullEmployee(int id, CancellationToken cancellationToken = default);

    }
}
