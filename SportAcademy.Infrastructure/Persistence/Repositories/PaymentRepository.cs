using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
