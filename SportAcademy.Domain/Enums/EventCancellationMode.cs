namespace SportAcademy.Domain.Enums;

// What happens to money already collected when an event is cancelled.
public enum EventCancellationMode
{
    // Give every payment on the event back (refund history + revenue report record it in the
    // month it happened). The invoice is cancelled.
    RefundPayments,
    // Keep what was collected (e.g. a non-refundable deposit) and waive the rest - the invoice
    // is closed at the amount paid, so nothing stays owed.
    KeepPayments,
}
