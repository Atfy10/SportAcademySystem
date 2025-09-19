using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface ITraineeRepository : IBaseRepository<Trainee, int>
    {
        Task<bool> IsSSNExistAsync(string ssn, CancellationToken cancellationToken = default);
        Task<Trainee?> GetFullTrainee(int id, CancellationToken cancellationToken = default);
    }
}
