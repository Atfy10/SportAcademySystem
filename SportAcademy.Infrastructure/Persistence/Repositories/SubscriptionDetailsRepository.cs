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
            IQueryable<SubscriptionDetails> query = ApplyBranchFilter(_context.SubscriptionDetails)
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
                query = query.Where(sd =>
                    sd.Trainee.FirstName.Contains(term)
                    || sd.Trainee.LastName.Contains(term)
                    || (sd.Trainee.FirstName + " " + sd.Trainee.LastName).Contains(term)
                    || sd.SportPrice.SportSubscriptionType.Sport.Name.Contains(term)
                    || sd.SportPrice.SportSubscriptionType.SubscriptionType.Name.Contains(term));
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
            // Unlike GetFullSubDetails()'s other callers (which fetch a specific, already
            // access-checked trainee's own history and must not hide subscriptions processed at
            // a different branch), this is a standalone subscriptions report/list - branch
            // restriction applies here the same way it does for GetAllPaginatedAsync above.
            var query = ApplyBranchFilter(GetFullSubDetails());

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
            //
            // Skip/Take must never run against GetFullSubDetails() itself: it Includes multiple
            // collection navigations (InvoiceLines, each with its own Allocations), and EF's join
            // explosion for those makes Skip/Take unreliable - rows can land on the wrong page or
            // be duplicated even though each row's own data is correct. So pagination happens
            // here against a plain, include-free query (branch-filtered, like
            // GetAllPaginatedAsync/GetReportAsync above), and GetFullSubDetails() is only used
            // afterward to hydrate the small, already-fixed set of ids for the current page.
            var baseQuery = ApplyBranchFilter(_context.SubscriptionDetails);

            var latestIdsQuery = baseQuery
                .GroupBy(sd => new { sd.TraineeId, sd.SportPrice.SportId, sd.SportPrice.BranchId })
                .Select(g => g.Max(sd => sd.Id));

            var query = baseQuery.Where(sd => latestIdsQuery.Contains(sd.Id));

            if (!string.IsNullOrWhiteSpace(term))
            {
                query = query.Where(sd =>
                    sd.Trainee.FirstName.Contains(term)
                    || sd.Trainee.LastName.Contains(term)
                    || (sd.Trainee.FirstName + " " + sd.Trainee.LastName).Contains(term)
                    || sd.SportPrice.SportSubscriptionType.Sport.Name.Contains(term)
                    || sd.SportPrice.SportSubscriptionType.SubscriptionType.Name.Contains(term));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var pageIds = await query
                .OrderByDescending(sd => sd.Id)
                .Skip(page.Skip)
                .Take(page.PageSize)
                .Select(sd => sd.Id)
                .ToListAsync(cancellationToken);

            if (pageIds.Count == 0)
                return ([], totalCount);

            var itemsById = await GetFullSubDetails()
                .Where(sd => pageIds.Contains(sd.Id))
                .ToDictionaryAsync(sd => sd.Id, cancellationToken);

            // Re-query above has no ordering of its own - restore the page's OrderByDescending(Id)
            // order from pageIds.
            var items = pageIds.Select(id => itemsById[id]).ToList();

            return (items, totalCount);
        }

        public async Task<List<SubscriptionDetailsDropdownDto>> GetActiveForTraineeDropdownAsync(int? traineeId, CancellationToken cancellationToken = default)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // ApplyBranchFilter is required here, not optional: SubscriptionDetails is
            // deliberately excluded from the automatic branch query filter (see
            // branchAutoFilterExclusions in ApplicationDbContext), on the understanding that
            // every hand-written list method applies the check itself - as the other methods in
            // this repository do. Without it a branch-restricted user is offered subscriptions
            // belonging to branches they can't see.
            var query = ApplyBranchFilter(_context.SubscriptionDetails)
                // EndDate, not just Status: Status is a stored flag that nothing expires on a
                // schedule - the only thing that flips Active -> Expired by date is the
                // ExecuteUpdateAsync inside GetSubDetailsStatsAsync, so a subscription's flag is
                // only as fresh as the last time someone loaded the stats. Offering one on the
                // flag alone means an enrollment can be created against an already-expired
                // subscription and is born expired. GetSubDetailsStatsAsync doesn't trust the
                // flag either - its "active" count is EndDate >= today && Status == Active.
                .Where(sd => sd.Status == SubscriptionStatus.Active
                    && sd.EndDate >= today
                    && !sd.IsDeleted
                    // A subscription already spent on an enrollment can't back another one -
                    // including a closed enrollment, which consumed it just as much as an open
                    // one did. (There is no unique index enforcing this at the database level;
                    // it's this query and CreateEnrollmentCommandHandler that keep it true.)
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

            var total = await ApplyBranchFilter(_context.SubscriptionDetails)
                .CountAsync(sd => !sd.IsDeleted, cancellationToken);
            var active = await ApplyBranchFilter(_context.SubscriptionDetails)
                .CountAsync(sd => !sd.IsDeleted && sd.EndDate >= today && sd.Status == SubscriptionStatus.Active, cancellationToken);
            var expired = await ApplyBranchFilter(_context.SubscriptionDetails)
                .CountAsync(sd => !sd.IsDeleted && sd.EndDate < today, cancellationToken);
            var expiringSoon = await ApplyBranchFilter(_context.SubscriptionDetails)
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
