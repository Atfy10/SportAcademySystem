using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Events;
using SportAcademy.Domain.Entities.Finance;

namespace SportAcademy.Application.Interfaces
{
    public record PaymentAllocationInput(int InvoiceId, decimal Amount);

    // Currency null = take it from the invoices being paid (they must all agree) - the caller
    // rarely knows better than the invoice what currency it was billed in.
    public record RecordPaymentInput(
        decimal Amount,
        int PaymentTypeId,
        int BranchId,
        string? Currency,
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
        // discountAmount/discountCodeId are 0/null for a plain (no discount code) invoice - see
        // SubscriptionCreationService, the only caller. GrandTotal = grossPrice - discountAmount.
        // dueDate: when the balance is expected to be settled - the collect date of a deposit, or
        // today when it's paid in full on the spot. A zero-total invoice (100% discount) is born
        // Paid: there is nothing to collect and no zero-amount payment is ever recorded for it.
        Task<Invoice> IssueSubscriptionInvoiceAsync(
            SubscriptionDetails subscription, decimal grossPrice, decimal discountAmount,
            int? discountCodeId, string currency, DateOnly dueDate, CancellationToken ct = default);

        // An event booking's invoice: an EventFee line for the rental price and, when decorated
        // with a fee, an EventDecoration line. The payer is the event's customer, not a trainee,
        // so their name/phone are copied onto the invoice (Invoice.PayerName/PayerPhone). The
        // event must already be saved (its Id goes on the lines). A zero-total event is born Paid.
        Task<Invoice> IssueEventInvoiceAsync(
            Event ev, EventCustomer customer, string currency, DateOnly dueDate, CancellationToken ct = default);

        // Re-prices an event's invoice after the event changed (price, decoration, customer or -
        // while nothing is paid - branch). Never lets the total drop below what's already been
        // collected. dueDate, when given, becomes the new collect date.
        Task ReviseEventInvoiceAsync(
            Invoice invoice, Event ev, EventCustomer customer, DateOnly? dueDate, CancellationToken ct = default);

        // Settles an event's invoice when the event is cancelled: nothing collected -> the
        // invoice is Cancelled; something collected and kept -> the rest is waived with an
        // Adjustment line so the invoice closes as Paid at the amount collected and nothing stays
        // owed. Refunding first (RefundPaymentAsync) and then calling this cancels it outright.
        Task CloseInvoiceForCancelledEventAsync(Invoice invoice, CancellationToken ct = default);

        Task<Payment> RecordPaymentAsync(RecordPaymentInput input, CancellationToken ct = default);

        // Gives back part of a payment. The refunded money is owed again on the invoice(s) it was
        // applied to (the invoice drops back to PartiallyPaid/Issued). newDueDate, when given,
        // becomes those invoices' due date so the reopened balance isn't instantly overdue.
        Task<PaymentRefund> RefundPaymentAsync(
            string paymentNumber, decimal amount, string reason, Guid? actingUserId,
            DateOnly? newDueDate, CancellationToken ct = default);

        // Reverses everything not yet refunded because the payment was recorded in error.
        Task<PaymentRefund> VoidPaymentAsync(
            string paymentNumber, string reason, Guid? actingUserId,
            DateOnly? newDueDate, CancellationToken ct = default);
    }
}
