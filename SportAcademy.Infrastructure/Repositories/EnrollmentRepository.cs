using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Infrastructure.Repositories
{
    public class EnrollmentRepository : BaseRepository<Enrollment, int>, IEnrollmentRepository
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Enrollment?> GetEnrollmentWithSession(int enrollmentId, CancellationToken cancellationToken = default)
            => await _context
                    .Enrollments
                    .Include(e => e.Session)
                    .SingleOrDefaultAsync(e => e.Id == enrollmentId, cancellationToken);
    }
}
