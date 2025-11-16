using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class SportPriceRepository : BaseRepository<SportPrice, int>, ISportPriceRepository
    {
        private readonly ApplicationDbContext _context;
        public SportPriceRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<bool> IsExistAsync(int branchId, int sportId, int subsTypeId, CancellationToken cancellationToken = default)
            => await _context.SportPrices.AnyAsync(
                sp => sp.BranchId == branchId
                    && sp.SportId == sportId
                    && sp.SubsTypeId == subsTypeId, cancellationToken);

        public async Task<SportPrice?> GetByKeyAsync(int branchId, int sportId, int subsTypeId, CancellationToken cancellationToken = default)
            => await _context.SportPrices.FirstOrDefaultAsync(
                sp => IsExistAsync(branchId, sportId, subsTypeId, cancellationToken).Result,
                cancellationToken);

        public async Task<List<SportPrice>> GetAllWithIncludesAsync(CancellationToken cancellationToken = default)
            => await _context.SportPrices
                    .Include(sp => sp.Branch)
                    .Include(sp => sp.SportSubscriptionType.Sport)
                    .Include(sp => sp.SportSubscriptionType.SubscriptionType)
                    .ToListAsync(cancellationToken);

        public async Task<SportPrice?> GetByKeyWithIncludesAsync(int branchId, int sportId, int subsTypeId, CancellationToken cancellationToken = default)
            => await _context.SportPrices
                    .Include(sp => sp.Branch)
                    .Include(sp => sp.SportSubscriptionType.Sport)
                    .Include(sp => sp.SportSubscriptionType.SubscriptionType)
                    .FirstOrDefaultAsync(
                        sp => IsExistAsync(branchId, sportId, subsTypeId, cancellationToken).Result,
                        cancellationToken);
    }
}
