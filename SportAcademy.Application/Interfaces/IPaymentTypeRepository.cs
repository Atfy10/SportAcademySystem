using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface IPaymentTypeRepository : IBaseRepository<PaymentType, int>
    {
        Task<List<PaymentType>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<bool> HasPaymentsAsync(int paymentTypeId, CancellationToken cancellationToken = default);
        Task<PaymentType?> GetDefaultAsync(CancellationToken cancellationToken = default);
        Task<PaymentType?> GetFirstActiveAsync(CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(CancellationToken cancellationToken = default);
        Task ClearDefaultFlagAsync(int? exceptId, CancellationToken cancellationToken = default);
    }
}
