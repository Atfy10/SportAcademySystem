using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class SubscriptionDetailsRepository : BaseRepository<SubscriptionDetails, int>, ISubscriptionDetailsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public SubscriptionDetailsRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
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

        public async Task<List<SubscriptionDetailsDropdownDto>> GetAllForDropdownAsync(CancellationToken cancellationToken = default)
            => await _context.SubscriptionDetails
                .AsNoTracking()
                .ProjectTo<SubscriptionDetailsDropdownDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

        public async Task<List<SubscriptionDetailsDropdownDto>> GetActiveForTraineeDropdownAsync(int? traineeId, CancellationToken cancellationToken = default)
        {
            var query = _context.SubscriptionDetails
                .Where(sd => sd.IsActive && !sd.IsDeleted);

            if (traineeId.HasValue)
            {
                query = query.Where(sd => sd.TraineeId == traineeId.Value);
            }

            return await query
                .AsNoTracking()
                .ProjectTo<SubscriptionDetailsDropdownDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public async Task<SubscriptionStatsDto> GetSubDetailsStatsAsync(CancellationToken cancellationToken = default)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var in15Days = today.AddDays(15);

            var total = await _context.SubscriptionDetails
                .CountAsync(sd => !sd.IsDeleted, cancellationToken);
            var active = await _context.SubscriptionDetails
                .CountAsync(sd => !sd.IsDeleted && sd.EndDate >= today, cancellationToken);
            var expired = await _context.SubscriptionDetails
                .CountAsync(sd => !sd.IsDeleted && sd.EndDate < today, cancellationToken);
            var expiringSoon = await _context.SubscriptionDetails
                .CountAsync(sd => !sd.IsDeleted && sd.EndDate >= today && sd.EndDate <= in15Days, cancellationToken);

            return new SubscriptionStatsDto
            {
                Total = total,
                Active = active,
                Expired = expired,
                ExpiringSoon = expiringSoon
            };
        }
    }
}
