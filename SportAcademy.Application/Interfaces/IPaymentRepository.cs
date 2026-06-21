using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Interfaces
{
    public interface IPaymentRepository : IBaseRepository<Payment, string>
    {
        Task<bool> IsRelatedToSubscriptionAsync(string paymentNumber, CancellationToken cancellationToken = default);
        Task<bool> ExistsForSubscriptionAsync(int subscriptionDetailsId, CancellationToken cancellationToken = default);
        Task<Payment> CreatePaymentAsync(string paymentNumber, PaymentMethod method, int branchId, CancellationToken cancellationToken = default);
    }
}
