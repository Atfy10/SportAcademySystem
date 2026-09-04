using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class SubscriptionDiscountRequestRepository
        : BaseRepository<SubscriptionDiscountRequest, int>, ISubscriptionDiscountRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionDiscountRequestRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        private IQueryable<SubscriptionDiscountRequest> WithIncludes() =>
            _context.SubscriptionDiscountRequests
                .Include(r => r.Trainee)
                .Include(r => r.SubscriptionType)
                .Include(r => r.Sport)
                .Include(r => r.Branch)
                .AsNoTracking();

        public async Task<(List<SubscriptionDiscountRequest> Items, int TotalCount)> GetPagedAsync(
            PageRequest page, SubscriptionDiscountRequestStatus? status, int? branchId,
            CancellationToken cancellationToken = default)
        {
            var query = WithIncludes();

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            if (branchId.HasValue)
                query = query.Where(r => r.BranchId == branchId.Value);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(r => r.RequestedAt)
                .Skip(page.Skip)
                .Take(page.PageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<SubscriptionDiscountRequest?> GetByIdWithIncludesAsync(int id, CancellationToken cancellationToken = default)
            => await WithIncludes().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }
}
