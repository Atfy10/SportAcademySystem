using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
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

        public async Task<PagedData<SubscriptionDetailsDto>> GetAllPaginatedAsync(PageRequest page, string? term = null, CancellationToken ct = default)
        {
            IQueryable<SubscriptionDetails> query = _context.SubscriptionDetails
                .Include(sd => sd.Trainee)
                .Include(sd => sd.SportPrice)
                    .ThenInclude(sp => sp.Branch)
                .Include(sd => sd.SportPrice)
                    .ThenInclude(sp => sp.SportSubscriptionType)
                        .ThenInclude(sst => sst.Sport)
                .Include(sd => sd.SportPrice)
                    .ThenInclude(sp => sp.SportSubscriptionType)
                        .ThenInclude(sst => sst.SubscriptionType)
                .Include(sd => sd.Payment)
                    .ThenInclude(p => p.Branch)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(term))
            {
                // SubscriptionType.Name is an enum (string-converted column) - EF Core can't
                // translate ToString()/Contains() on it into SQL, so it's left out of search
                // here (trainee/sport name cover the common case; subscription-type search
                // would need its own DB-side string comparison to be added safely).
                query = query.Where(sd =>
                    sd.Trainee.FirstName.Contains(term)
                    || sd.Trainee.LastName.Contains(term)
                    || (sd.Trainee.FirstName + " " + sd.Trainee.LastName).Contains(term)
                    || sd.SportPrice.SportSubscriptionType.Sport.Name.Contains(term));
            }

            var totalCount = await query.CountAsync(ct);
            var pageEntities = await query
                .OrderByDescending(sd => sd.Id)
                .Skip(page.Skip)
                .Take(page.PageSize)
                .ToListAsync(ct);

            return new PagedData<SubscriptionDetailsDto>
            {
                Items = pageEntities.Select(SubscriptionDetailsMapper.ToDto).ToList(),
                TotalCount = totalCount,
                Page = page.Page,
                PageSize = page.PageSize,
            };
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
                .Select(sd => sd.SportPrice.SportSubscriptionType.SubscriptionType.DaysPerMonth
                    * sd.SportPrice.SportSubscriptionType.SubscriptionType.NumberOfMonths)
                .SingleOrDefaultAsync(cancellationToken);

        public async Task<List<SubscriptionDetails>?> GetSubscriptionDetailsForTraineeAsync(int traineeId, CancellationToken cancellationToken = default)
            => await _context.SubscriptionDetails
                .Where(sd => sd.TraineeId == traineeId)
                .ToListAsync(cancellationToken);

        public async Task<List<SubscriptionDetails>?> GetActiveSubscriptionDetailsForTraineeAsync(int traineeId, CancellationToken cancellationToken = default)
            => await _context.SubscriptionDetails
                .Where(sd => sd.TraineeId == traineeId && sd.Status == SubscriptionStatus.Active)
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
                    .ThenInclude(sp => sp.SportBranch)
                        .ThenInclude(sb => sb.Branch)
                .Include(sd => sd.Payment)
                    .ThenInclude(p => p.Branch);

        public async Task<List<SubscriptionDetailsDropdownDto>> GetAllForDropdownAsync(CancellationToken cancellationToken = default)
            => await _context.SubscriptionDetails
                .AsNoTracking()
                .ProjectTo<SubscriptionDetailsDropdownDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

        public async Task<(List<SubscriptionDetails> Items, int TotalCount)> GetLatestSubscriptionsAsync(
            PageRequest page, string? term = null, CancellationToken cancellationToken = default)
        {
            // GroupBy(...).Select(g => g.OrderBy(...).First()) does not translate to SQL Server
            // ("could not be translated") - EF Core can't turn "order the group then take the
            // first row" into a correlated APPLY for this shape, so this endpoint threw on every
            // call. Rewritten as "ids of the max row per group, then re-query by those ids" -
            // GroupBy+Max and Where+Contains(subquery) are both well within EF's supported
            // translation set.
            var latestIdsQuery = GetFullSubDetails()
                .GroupBy(sd => new { sd.TraineeId, sd.SportPrice.SportId, sd.SportPrice.BranchId })
                .Select(g => g.Max(sd => sd.Id));

            var query = GetFullSubDetails().Where(sd => latestIdsQuery.Contains(sd.Id));

            if (!string.IsNullOrWhiteSpace(term))
            {
                // SubscriptionType.Name is an enum (string-converted column) - EF Core can't
                // translate it into SQL, same limitation as GetAllPaginatedAsync above.
                query = query.Where(sd =>
                    sd.Trainee.FirstName.Contains(term)
                    || sd.Trainee.LastName.Contains(term)
                    || (sd.Trainee.FirstName + " " + sd.Trainee.LastName).Contains(term)
                    || sd.SportPrice.SportSubscriptionType.Sport.Name.Contains(term));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(sd => sd.Id)
                .Skip(page.Skip)
                .Take(page.PageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<List<SubscriptionDetailsDropdownDto>> GetActiveForTraineeDropdownAsync(int? traineeId, CancellationToken cancellationToken = default)
        {
            var query = _context.SubscriptionDetails
                .Where(sd => sd.Status == SubscriptionStatus.Active && !sd.IsDeleted);

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

            await _context.SubscriptionDetails
                .Where(sd => sd.Status == SubscriptionStatus.Active && sd.EndDate < today)
                .ExecuteUpdateAsync(s => s.SetProperty(sd => sd.Status, SubscriptionStatus.Expired), cancellationToken);

            var total = await _context.SubscriptionDetails
                .CountAsync(sd => !sd.IsDeleted, cancellationToken);
            var active = await _context.SubscriptionDetails
                .CountAsync(sd => !sd.IsDeleted && sd.EndDate >= today && sd.Status == SubscriptionStatus.Active, cancellationToken);
            var expired = await _context.SubscriptionDetails
                .CountAsync(sd => !sd.IsDeleted && sd.EndDate < today, cancellationToken);
            var expiringSoon = await _context.SubscriptionDetails
                .CountAsync(sd => !sd.IsDeleted && sd.EndDate >= today && sd.EndDate <= today.AddDays(15), cancellationToken);

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
