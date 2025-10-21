using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface IPaymentRepository : IBaseRepository<Domain.Entities.Payment, string>
    {
        Task<bool> IsExistByPaymentAsync(string paymentNumber, CancellationToken cancellationToken = default);
    }
}
