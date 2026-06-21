using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class PaymentRepository : BaseRepository<Payment, string>, IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> IsRelatedToSubscriptionAsync(string paymentNumber, CancellationToken cancellationToken = default)
            => await _context.Payments
                .AnyAsync(p => p.PaymentNumber == paymentNumber && p.SubscriptionDetails != null,
                    cancellationToken);

        public async Task<bool> ExistsForSubscriptionAsync(int subscriptionDetailsId, CancellationToken cancellationToken = default)
            => await _context.Payments
                .AnyAsync(p => p.SubscriptionDetails != null && p.SubscriptionDetails.Id == subscriptionDetailsId, cancellationToken);

        public async Task<Payment> CreatePaymentAsync(string paymentNumber, PaymentMethod method, int branchId, CancellationToken cancellationToken = default)
        {
            var payment = new Payment
            {
                PaymentNumber = paymentNumber,
                Method = method,
                BranchId = branchId,
                PaidDate = DateTime.UtcNow
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync(cancellationToken);
            return payment;
        }
    }
}
