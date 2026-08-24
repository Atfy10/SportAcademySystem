using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.FinanceDtos;

public record PaymentReceiptAllocationDto(int InvoiceId, string InvoiceNumber, decimal Amount);

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
    List<PaymentReceiptAllocationDto> Allocations);
