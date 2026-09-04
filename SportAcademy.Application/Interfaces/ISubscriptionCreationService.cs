using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    // The actual "create a subscription" logic (SportPrice lookup, entity creation, SportTrainee
    // back-fill, renewal carry-forward, invoice issuance, payment recording) - extracted out of
    // CreateSubscriptionDetailsCommandHandler so both the immediate no-discount path (that
    // handler) and the post-approval path (ApproveSubscriptionDiscountRequestCommandHandler) can
    // call the exact same logic instead of two copies drifting apart.
    //
    // Takes discountPercentage (not a pre-computed amount) so the discount is always calculated
    // against the SAME SportPrice lookup this method makes internally - a caller computing the
    // amount from its own separate price lookup could disagree with this one if the price
    // changed between the two calls. null/null for the plain (no discount code) path.
    public interface ISubscriptionCreationService
    {
        Task<SubscriptionDetails> CreateAsync(
            int traineeId, int subscriptionTypeId, int sportId, int branchId,
            DateOnly startDate, DateOnly endDate, int paymentTypeId,
            decimal? discountPercentage, int? discountCodeId, Guid? actingUserId,
            CancellationToken ct = default);
    }
}
