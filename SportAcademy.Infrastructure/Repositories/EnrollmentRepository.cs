using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.DBContext;

namespace SportAcademy.Infrastructure.Repositories
{
    public class EnrollmentRepository : BaseRepository<Enrollment, int>, IEnrollmentRepository
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<int> GetTotalSessionsAllowed(int enrollmentId, CancellationToken cancellationToken)
            => await _context.Enrollments
                .Where(e => e.Id == enrollmentId)
                .Select(e => e.SubscriptionDetails.SportSubscriptionType.SubscriptionType.DaysPerMonth)
                .SingleOrDefaultAsync(cancellationToken);
    }
}
