using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Interfaces
{
    // Everything needed to create one subscription. DiscountPercentage (not a pre-computed
    // amount) so the discount is always calculated against the SAME SportPrice lookup the
    // service makes internally - a caller computing the amount from its own separate price lookup
    // could disagree with it if the price changed between the two calls. null/null for the plain
    // (no discount code) path.
    //
    // DepositAmount/BalanceDueDate: null/null = paid in full now. With a deposit, only that much is
    // recorded as paid and the rest stays outstanding on the invoice until BalanceDueDate (the
    // "collect date"), after which it shows as overdue.
    public record SubscriptionCreationRequest(
        int TraineeId,
        int SubscriptionTypeId,
        int SportId,
        int BranchId,
        DateOnly StartDate,
        TraineeGroupType GroupType,
        IReadOnlyCollection<DayOfWeek> TrainingDays,
        int PaymentTypeId,
        decimal? DiscountPercentage,
        int? DiscountCodeId,
        Guid? ActingUserId,
        decimal? DepositAmount = null,
        DateOnly? BalanceDueDate = null);

    // Payment is null when nothing was collected at creation (a zero-total invoice).
    public record SubscriptionCreationResult(SubscriptionDetails Subscription, Invoice Invoice, Payment? Payment);

    // The actual "create a subscription" logic (SportPrice lookup, entity creation, SportTrainee
    // back-fill, renewal carry-forward, invoice issuance, payment recording) - extracted out of
    // CreateSubscriptionDetailsCommandHandler so both the immediate no-discount path (that
    // handler) and the post-approval path (ApproveSubscriptionDiscountRequestCommandHandler) can
    // call the exact same logic instead of two copies drifting apart.
    public interface ISubscriptionCreationService
    {
        // The end date is not an input: it's derived here from the subscription type's session
        // count walked across TrainingDays (see TrainingScheduleService.ComputeEndDate), so a
        // caller can't submit a date that disagrees with the schedule the trainee will actually
        // train on. GroupType selects which SportPrice row applies - public and private training
        // for the same sport/branch/type are priced separately.
        //
        // beforeCommit runs inside the same transaction, after everything is written - for a
        // caller whose own bookkeeping must succeed or fail together with the subscription (the
        // discount-request approval marking itself Approved).
        Task<SubscriptionCreationResult> CreateAsync(
            SubscriptionCreationRequest request,
            Func<SubscriptionCreationResult, CancellationToken, Task>? beforeCommit = null,
            CancellationToken ct = default);
    }
}
