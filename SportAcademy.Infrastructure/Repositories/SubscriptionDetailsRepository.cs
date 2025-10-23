using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.DBContext;

namespace SportAcademy.Infrastructure.Repositories
{
    public class SubscriptionDetailsRepository : BaseRepository<SubscriptionDetails, int>, ISubscriptionDetailsRepository
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionDetailsRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<SubscriptionDetails?> GetSubscriptionDetailsWithSubTypeAsync(int subscriptionId, CancellationToken cancellationToken = default)
            => await _context.SubscriptionDetails
                .Include(sd => sd.SportSubscriptionType.SubscriptionType)
                .SingleOrDefaultAsync(sd => sd.Id == subscriptionId, cancellationToken);

        public async Task<int> GetTotalSessionsAllowed(int subDetailsId, CancellationToken cancellationToken)
            => await _context.SubscriptionDetails
                .Where(sd => sd.Id == subDetailsId)
                .Select(sd => sd.SportSubscriptionType.SubscriptionType.DaysPerMonth)
                .SingleOrDefaultAsync(cancellationToken);
    }
}
