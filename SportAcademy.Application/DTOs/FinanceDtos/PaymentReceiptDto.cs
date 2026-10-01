using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.FinanceDtos;

// Everything a printable customer receipt needs: who paid for what, how much of the bill this
// settled, what is still owed and by when, and any money given back since.
public record PaymentReceiptAllocationDto(
    int InvoiceId,
    string InvoiceNumber,
    decimal Amount,
    decimal ReversedAmount = 0,
    decimal InvoiceTotal = 0,
    decimal InvoiceDiscount = 0,
    decimal InvoicePaidToDate = 0,
    decimal InvoiceBalance = 0,
    DateOnly? InvoiceDueDate = null,
    string? SportName = null,
    string? SubscriptionTypeName = null,
    DateOnly? SubscriptionStartDate = null,
    DateOnly? SubscriptionEndDate = null,
    // Set when the invoice billed an event booking instead of a subscription. EventStartsAt is
    // the academy's wall-clock time.
    int? EventId = null,
    string? EventTitle = null,
    DateTime? EventStartsAt = null);

public record PaymentReceiptTraineeDto(int Id, string FullName, string? PhoneNumber, string? Code);

// Who paid when it wasn't a trainee (an event's customer), from the name/phone copied onto the
// invoice.
public record PaymentReceiptPayerDto(string FullName, string? PhoneNumber);

public record PaymentRefundDto(
    int Id,
    PaymentRefundKind Kind,
    decimal Amount,
    string Reason,
    DateTime RefundedAt,
    string? RefundedByName);

public record PaymentReceiptDto(
    string PaymentNumber,
    decimal Amount,
    decimal RefundedAmount,
    string PaymentTypeName,
    PaymentStatus Status,
    DateTime PaidDate,
    string BranchName,
    string Currency,
    string? Reference,
    string? Notes,
    List<PaymentReceiptAllocationDto> Allocations,
    List<PaymentReceiptTraineeDto>? Trainees = null,
    string? RecordedByName = null,
    List<PaymentRefundDto>? Refunds = null,
    List<PaymentReceiptPayerDto>? Payers = null);
