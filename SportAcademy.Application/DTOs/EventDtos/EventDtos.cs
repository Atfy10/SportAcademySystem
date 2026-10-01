using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.EventDtos;

// StartsAt/EndsAt are UTC instants ("...Z"); StartsAtLocal/EndsAtLocal are the same moments as
// the academy's wall clock (its configured time zone, no offset) - what staff typed and expect to
// read back, whatever zone their own device is in.
// Billed/AmountPaid/Balance come from the event's invoice. Billed can be less than TotalPrice
// after a cancellation that kept a deposit (the rest was waived), and is 0 for a cancelled
// invoice. PaymentState is null once the event is cancelled.
public record EventDto(
    int Id,
    string Title,
    int BranchId,
    string BranchName,
    int CustomerId,
    string CustomerName,
    string CustomerPhone,
    string NationalityCategoryName,
    bool WithDecorations,
    decimal Price,
    decimal DecorationFee,
    decimal TotalPrice,
    int Capacity,
    DateTime StartsAt,
    DateTime EndsAt,
    DateTime StartsAtLocal,
    DateTime EndsAtLocal,
    EventStatus Status,
    string? Notes,
    int? InvoiceId,
    string? InvoiceNumber,
    string Currency,
    decimal Billed,
    decimal AmountPaid,
    decimal Balance,
    DateOnly? DueDate,
    EventPaymentState? PaymentState,
    Guid CreatedByUserId,
    string? CreatedByName,
    DateTime CreatedAt,
    DateTime? CancelledAt,
    string? CancelReason,
    string? CancelledByName,
    // When the door starts letting ticket holders in (EventEntryRules.OpensBefore before the
    // start), on the academy's wall clock. The tickets themselves: GET api/events/{id}/tickets.
    DateTime EntryOpensAtLocal);

// One payment applied to the event's invoice. Amount is what it put on this invoice;
// ReversedAmount is what has since been refunded/voided back out of it.
public record EventPaymentDto(
    string PaymentNumber,
    DateTime PaidDate,
    decimal Amount,
    decimal ReversedAmount,
    string PaymentTypeName,
    PaymentStatus Status);

public record EventDetailsDto(EventDto Event, List<EventPaymentDto> Payments);

public record EventOverlapDto(int Id, string Title, DateTime StartsAtLocal, DateTime EndsAtLocal, string CustomerName);

public record EventsReportTotalsDto(
    int EventCount,
    int CancelledCount,
    int TotalCapacity,
    int DecoratedCount,
    decimal TotalBilled,
    decimal TotalPaid,
    decimal TotalBalance);

public record EventsReportDto(
    DateOnly? From,
    DateOnly? To,
    string Currency,
    List<EventDto> Rows,
    EventsReportTotalsDto Totals,
    bool Truncated);

// LastEventAt is the academy's wall-clock time. EventCount/LastEventAt/totals cover the customer's non-cancelled events the caller can see
// (a branch-restricted user only sees their branches' events).
public record EventCustomerDto(
    int Id,
    string FullName,
    string PhoneNumber,
    int NationalityCategoryId,
    string NationalityCategoryName,
    string? Notes,
    bool IsActive,
    int EventCount,
    DateTime? LastEventAt,
    decimal TotalBilled,
    decimal TotalPaid,
    decimal Balance,
    DateTime CreatedAt);
