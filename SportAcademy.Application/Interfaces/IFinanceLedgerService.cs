using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Finance;

namespace SportAcademy.Application.Interfaces
{
    public record PaymentAllocationInput(int InvoiceId, decimal Amount);

    public record RecordPaymentInput(
        decimal Amount,
        int PaymentTypeId,
        int BranchId,
        string Currency,
        string? Reference,
        string? Notes,
        Guid? RecordedByUserId,
        IReadOnlyList<PaymentAllocationInput> Allocations);

    // The only code allowed to create an Invoice, record a Payment, or change either one's
    // balance/status - everything else (handlers, controllers) goes through this so
    // "Invoice.AmountPaid == sum of its allocations" can never drift. Model on
    // SubDetailsManagementService for the existing precedent of an Application-layer domain
    // service composed from repositories rather than a raw DbContext.
    public interface IFinanceLedgerService
    {
        Task<Invoice> IssueSubscriptionInvoiceAsync(
            SubscriptionDetails subscription, decimal price, string currency, CancellationToken ct = default);

        Task<Payment> RecordPaymentAsync(RecordPaymentInput input, CancellationToken ct = default);

        Task RefundPaymentAsync(string paymentNumber, decimal amount, CancellationToken ct = default);

        Task VoidPaymentAsync(string paymentNumber, CancellationToken ct = default);
    }
}
