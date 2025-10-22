using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface IEnrollmentRepository : IBaseRepository<Enrollment, int>
    {
        Task<int> GetTotalSessionsAllowed(int enrollmentId, CancellationToken cancellationToken);
    }
}
