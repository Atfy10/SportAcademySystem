using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface IEnrollmentRepository : IBaseRepository<Enrollment, int>
    {
        Task<Enrollment?> GetEnrollmentWithSession(int enrollmentId, CancellationToken cancellationToken);
    }
}
