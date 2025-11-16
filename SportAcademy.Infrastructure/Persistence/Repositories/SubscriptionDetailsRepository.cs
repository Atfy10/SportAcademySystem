using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class SubscriptionDetailsRepository : BaseRepository<SubscriptionDetails, int>, ISubscriptionDetailsRepository
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionDetailsRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<SubscriptionDetails>?> GetAllFullSubDetailsAsync(CancellationToken cancellationToken = default)
            => await GetFullSubDetails().ToListAsync(cancellationToken);

        public async Task<SubscriptionDetails?> GetFullSubscriptionDetails(int subscriptionId, CancellationToken cancellationToken = default)
            => await GetFullSubDetails()
                .SingleOrDefaultAsync(sd => sd.Id == subscriptionId, cancellationToken);

        public async Task<SubscriptionDetails?> GetSubscriptionDetailsWithSubTypeAsync(int subscriptionId, CancellationToken cancellationToken = default)
            => await _context.SubscriptionDetails
                .Include(sd => sd.SportPrice.SportSubscriptionType.SubscriptionType)
                .SingleOrDefaultAsync(sd => sd.Id == subscriptionId, cancellationToken);

        public async Task<int> GetTotalSessionsAllowed(int subDetailsId, CancellationToken cancellationToken)
            => await _context.SubscriptionDetails
                .Where(sd => sd.Id == subDetailsId)
                .Select(sd => sd.SportPrice.SportSubscriptionType.SubscriptionType.DaysPerMonth)
                .SingleOrDefaultAsync(cancellationToken);

        public async Task<List<SubscriptionDetails>?> GetSubscriptionDetailsForTraineeAsync(int traineeId, CancellationToken cancellationToken = default)
            => await _context.SubscriptionDetails
                .Where(sd => sd.TraineeId == traineeId)
                .ToListAsync(cancellationToken);

        public async Task<List<SubscriptionDetails>?> GetActiveSubscriptionDetailsForTraineeAsync(int traineeId, CancellationToken cancellationToken = default)
            => await _context.SubscriptionDetails
                .Where(sd => sd.TraineeId == traineeId && sd.IsActive)
                .ToListAsync(cancellationToken);

        private IQueryable<SubscriptionDetails> GetFullSubDetails()
            => _context.SubscriptionDetails
                .Include(sd => sd.Trainee)
                    .ThenInclude(t => t.AppUser)
                .Include(sd => sd.SportPrice)
                    .ThenInclude(sp => sp.SportSubscriptionType)
                        .ThenInclude(sst => sst.SubscriptionType)
                .Include(sd => sd.SportPrice)
                    .ThenInclude(sp => sp.SportSubscriptionType)
                        .ThenInclude(sst => sst.Sport)
                .Include(sd => sd.SportPrice)
                    .ThenInclude(sp => sp.Branch)
                .Include(sd => sd.Payment)
                    .ThenInclude(p => p.Branch);
    }
}
