using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class CoachRepository : BaseRepository<Coach, int>, ICoachRepository
    {
        private readonly ApplicationDbContext _context;

        public CoachRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<int> CountAsync(CancellationToken cancellationToken)
        {
            return await _context.Coachs.CountAsync(cancellationToken);
        }
        public async Task<double?> GetAverageRatingAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Coachs
                .Select(x => (double?)x.Rate)
                .AverageAsync(cancellationToken);
        }
    }
}
