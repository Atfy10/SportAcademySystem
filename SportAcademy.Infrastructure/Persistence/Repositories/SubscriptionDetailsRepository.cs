using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class SubscriptionDetailsRepository : BaseRepository<SubscriptionDetails, int>, ISubscriptionDetailsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentLanguageProvider _languageProvider;

        public SubscriptionDetailsRepository(ApplicationDbContext context, IMapper mapper, ICurrentLanguageProvider languageProvider)
            : base(context, mapper, languageProvider)
        {
            _context = context;
            _mapper = mapper;
            _languageProvider = languageProvider;
        }

        public async Task<PagedData<SubscriptionDetailsDto>> GetAllPaginatedAsync(PageRequest page, string? term = null, CancellationToken ct = default)
        {
            IQueryable<SubscriptionDetails> query = _context.SubscriptionDetails
                .Include(sd => sd.Trainee)
                .Include(sd => sd.SportPrice)
                    .ThenInclude(sp => sp.Branch)
                        .ThenInclude(b => b.Translations)
                .Include(sd => sd.SportPrice)
                    .ThenInclude(sp => sp.SportSubscriptionType)
                        .ThenInclude(sst => sst.Sport)
                            .ThenInclude(sp => sp.Translations)
                .Include(sd => sd.SportPrice)
                    .ThenInclude(sp => sp.SportSubscriptionType)
                        .ThenInclude(sst => sst.SubscriptionType)
                .Include(sd => sd.InvoiceLines)
                    .ThenInclude(l => l.Invoice)
                        .ThenInclude(i => i.Allocations)
                            .ThenInclude(a => a.Payment)
                                .ThenInclude(p => p.Branch)
                                    .ThenInclude(b => b.Translations)
                .Include(sd => sd.InvoiceLines)
                    .ThenInclude(l => l.Invoice)
                        .ThenInclude(i => i.Allocations)
                            .ThenInclude(a => a.Payment)
                                .ThenInclude(p => p.PaymentType)
                                    .ThenInclude(pt => pt.Translations)
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
                Items = pageEntities.Select(sd => SubscriptionDetailsMapper.ToDto(sd, _languageProvider.Language)).ToList(),
                TotalCount = totalCount,
                Page = page.Page,
                PageSize = page.PageSize,
            };
        }

        public async Task<PagedData<SubscriptionDetailsDto>> GetReportAsync(
            DateTime? from, DateTime? to, int? branchId, int? sportId, SubscriptionStatus? status,
            PageRequest? page, CancellationToken ct = default)
        {
            var query = GetFullSubDetails();

            if (from.HasValue) query = query.Where(sd => sd.EndDate >= DateOnly.FromDateTime(from.Value));
            if (to.HasValue) query = query.Where(sd => sd.StartDate <= DateOnly.FromDateTime(to.Value));
            if (branchId.HasValue) query = query.Where(sd => sd.BranchId == branchId.Value);
            if (sportId.HasValue) query = query.Where(sd => sd.SportId == sportId.Value);
            if (status.HasValue) query = query.Where(sd => sd.Status == status.Value);

            var totalCount = await query.CountAsync(ct);
            var take = page?.PageSize ?? 5000;
            var entities = await query
                .OrderByDescending(sd => sd.Id)
                .Skip(page?.Skip ?? 0)
                .Take(take)
                .ToListAsync(ct);

            return new PagedData<SubscriptionDetailsDto>
            {
                Items = entities.Select(sd => SubscriptionDetailsMapper.ToDto(sd, _languageProvider.Language)).ToList(),
                TotalCount = totalCount,
                Page = page?.Page ?? 1,
                PageSize = take,
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

        public async Task<List<SubscriptionDetails>> GetAllFullSubDetailsForTraineeIdAsync(int traineeId, CancellationToken cancellationToken = default)
            => await GetFullSubDetails()
                .Where(sd => sd.TraineeId == traineeId)
                .OrderByDescending(sd => sd.StartDate)
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
                            .ThenInclude(sp => sp.Translations)
                .Include(sd => sd.SportPrice)
                    .ThenInclude(sp => sp.Branch)
                        .ThenInclude(b => b.Translations)
                .Include(sd => sd.SportPrice)
                    .ThenInclude(sp => sp.SportBranch)
                        .ThenInclude(sb => sb.Branch)
                .Include(sd => sd.InvoiceLines)
                    .ThenInclude(l => l.Invoice)
                        .ThenInclude(i => i.Allocations)
                            .ThenInclude(a => a.Payment)
                                .ThenInclude(p => p.Branch)
                                    .ThenInclude(b => b.Translations)
                .Include(sd => sd.InvoiceLines)
                    .ThenInclude(l => l.Invoice)
                        .ThenInclude(i => i.Allocations)
                            .ThenInclude(a => a.Payment)
                                .ThenInclude(p => p.PaymentType)
                                    .ThenInclude(pt => pt.Translations);

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
                .Where(sd => sd.Status == SubscriptionStatus.Active && !sd.IsDeleted
                    // A subscription already claimed by an enrollment (its required 1:1 FK)
                    // can't be picked for a new one - the unique constraint would reject it.
                    && !_context.Enrollments.Any(e => e.SubscriptionDetailsId == sd.Id));

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
