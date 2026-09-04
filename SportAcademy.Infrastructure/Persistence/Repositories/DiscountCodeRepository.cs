using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class DiscountCodeRepository : BaseRepository<DiscountCode, int>, IDiscountCodeRepository
    {
        private readonly ApplicationDbContext _context;

        public DiscountCodeRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<DiscountCode>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _context.DiscountCodes
                .AsNoTracking()
                .OrderBy(c => c.Code)
                .ToListAsync(cancellationToken);

        public async Task<DiscountCode?> GetActiveByCodeAsync(string normalizedCode, CancellationToken cancellationToken = default)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            return await _context.DiscountCodes
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.Code == normalizedCode &&
                    c.IsActive &&
                    (c.ExpiresAt == null || c.ExpiresAt >= today),
                    cancellationToken);
        }

        public async Task<bool> HasInvoiceLinesAsync(int discountCodeId, CancellationToken cancellationToken = default)
            => await _context.InvoiceLines.AnyAsync(l => l.DiscountCodeId == discountCodeId, cancellationToken);
    }
}
