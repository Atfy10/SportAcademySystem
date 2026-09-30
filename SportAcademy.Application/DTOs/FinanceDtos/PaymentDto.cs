using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.FinanceDtos;

// TraineeName is who the payment was for (the trainee on the invoice it settled) - what staff
// actually look a payment up by. A payment covering several trainees' invoices (a family paying
// together) names the first and reports the rest through TraineeCount.
public record PaymentDto(
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
    int? TraineeId = null,
    string? TraineeName = null,
    int TraineeCount = 0,
    List<string>? InvoiceNumbers = null);
