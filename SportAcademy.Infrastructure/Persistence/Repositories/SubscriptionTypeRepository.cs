using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
	public class SubscriptionTypeRepository : BaseRepository<SubscriptionType, int>, ISubscriptionTypeRepository
	{
		private readonly ApplicationDbContext _context;

		public SubscriptionTypeRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<List<SubscriptionType>> GetAllWithSportsAsync(CancellationToken cancellationToken = default)
			=> await _context.SubscriptionTypes
				.AsNoTracking()
				.Include(st => st.Sports)
					.ThenInclude(sst => sst.Sport)
				.ToListAsync(cancellationToken);

		public async Task<SubscriptionType?> GetByIdWithSportsAsync(int id, CancellationToken cancellationToken = default)
			=> await _context.SubscriptionTypes
				.Include(st => st.Sports)
					.ThenInclude(sst => sst.Sport)
				.SingleOrDefaultAsync(st => st.Id == id, cancellationToken);
	}
}
