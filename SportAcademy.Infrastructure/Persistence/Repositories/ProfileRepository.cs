using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class ProfileRepository : BaseRepository<Profile, string>, IProfileRepository
    {
        public ProfileRepository(ApplicationDbContext context) : base(context) { }
    }
}
