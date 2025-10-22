using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.DBContext;

namespace SportAcademy.Infrastructure.Repositories
{
    public class TraineeGroupRepository : BaseRepository<TraineeGroup, int>, ITraineeGroupRepository
    {
        private readonly ApplicationDbContext _context;

        public TraineeGroupRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
