using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.DBContext;

namespace SportAcademy.Infrastructure.Repositories
{
    public class SessionOccurrenceRepository : BaseRepository<SessionOccurrence, int>, ISessionOccurrenceRepository
    {
        private readonly ApplicationDbContext _context;

        public SessionOccurrenceRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
