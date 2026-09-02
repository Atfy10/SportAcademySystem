using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class ProfileRepository : BaseRepository<Profile, string>, IProfileRepository
    {
        private readonly ApplicationDbContext _context;

        public ProfileRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Profile?> GetByAppUserIdAsync(Guid appUserId, CancellationToken cancellationToken = default)
            => await _context.Set<Profile>().SingleOrDefaultAsync(p => p.AppUserId == appUserId, cancellationToken);
    }
}
