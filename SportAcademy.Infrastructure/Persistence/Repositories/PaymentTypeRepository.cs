using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class PaymentTypeRepository : BaseRepository<PaymentType, int>, IPaymentTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentTypeRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<PaymentType>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _context.PaymentTypes
                .AsNoTracking()
                .OrderBy(pt => pt.Name)
                .ToListAsync(cancellationToken);

        public async Task<bool> HasPaymentsAsync(int paymentTypeId, CancellationToken cancellationToken = default)
            => await _context.Payments.AnyAsync(p => p.PaymentTypeId == paymentTypeId, cancellationToken);

        public async Task<PaymentType?> GetDefaultAsync(CancellationToken cancellationToken = default)
            => await _context.PaymentTypes.SingleOrDefaultAsync(pt => pt.IsDefault, cancellationToken);

        public async Task<PaymentType?> GetFirstActiveAsync(CancellationToken cancellationToken = default)
            => await _context.PaymentTypes
                .Where(pt => pt.IsActive)
                .OrderBy(pt => pt.Id)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
            => await _context.PaymentTypes.AnyAsync(cancellationToken);

        public async Task ClearDefaultFlagAsync(int? exceptId, CancellationToken cancellationToken = default)
        {
            await _context.PaymentTypes
                .Where(pt => pt.IsDefault && (exceptId == null || pt.Id != exceptId.Value))
                .ExecuteUpdateAsync(s => s.SetProperty(pt => pt.IsDefault, false), cancellationToken);
        }
    }
}
