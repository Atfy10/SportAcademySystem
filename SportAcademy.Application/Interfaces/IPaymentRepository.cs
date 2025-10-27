using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface IPaymentRepository : IBaseRepository<Payment, string>
    {
        Task<bool> IsRelatedToSubscriptionAsync(string paymentNumber, CancellationToken cancellationToken = default);

    }
}
