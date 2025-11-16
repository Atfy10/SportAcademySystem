using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class TraineeRepository : BaseRepository<Trainee, int>, ITraineeRepository
    {
        private readonly ApplicationDbContext _context;

        public TraineeRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Trainee?> GetFullTrainee(int id, CancellationToken cancellationToken = default)
            => await _context.Trainees
                .Where(t => t.Id == id)
                .Include(t => t.AppUser)
                .SingleOrDefaultAsync(cancellationToken);

        public async Task<bool> IsSSNExistAsync(string ssn, CancellationToken cancellationToken = default)
            => await _context.Trainees.AnyAsync(t => t.SSN == ssn, cancellationToken);
	}
}
