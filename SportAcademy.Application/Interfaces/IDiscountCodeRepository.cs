using SportAcademy.Domain.Entities.Finance;

namespace SportAcademy.Application.Interfaces
{
    public interface IDiscountCodeRepository : IBaseRepository<DiscountCode, int>
    {
        Task<List<DiscountCode>> GetAllAsync(CancellationToken cancellationToken = default);

        // Case-insensitive match on the normalized code, filtered to currently redeemable
        // (IsActive && (ExpiresAt is null || ExpiresAt >= today)) - the single source of truth
        // both the validate-preview endpoint and the authoritative request/approval checks use,
        // so they can never disagree about what's valid.
        Task<DiscountCode?> GetActiveByCodeAsync(string normalizedCode, CancellationToken cancellationToken = default);

        Task<bool> HasInvoiceLinesAsync(int discountCodeId, CancellationToken cancellationToken = default);
    }
}
