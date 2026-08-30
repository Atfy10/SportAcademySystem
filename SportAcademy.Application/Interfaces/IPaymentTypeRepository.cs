using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface IPaymentTypeRepository : IBaseRepository<PaymentType, int>
    {
        Task<List<PaymentType>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PaymentType?> GetByIdWithTranslationAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Tracked (not AsNoTracking) with Translations eagerly loaded - for the Update handler,
        /// so it can safely add/update/remove a translation row on the loaded collection without
        /// EF mistaking an unloaded collection for "no translation exists yet". Deliberately
        /// separate from GetByIdWithTranslationAsync, which is read-only/no-tracking.
        /// </summary>
        Task<PaymentType?> GetByIdWithTranslationsTrackedAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> HasPaymentsAsync(int paymentTypeId, CancellationToken cancellationToken = default);
        Task<PaymentType?> GetDefaultAsync(CancellationToken cancellationToken = default);
        Task<PaymentType?> GetFirstActiveAsync(CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(CancellationToken cancellationToken = default);
        Task ClearDefaultFlagAsync(int? exceptId, CancellationToken cancellationToken = default);
    }
}
