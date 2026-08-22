using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.FinanceDtos;

public record InvoiceLineDto(string Type, string Description, int Quantity, decimal UnitPrice, decimal LineTotal);

public record InvoiceDto(
    int Id,
    string InvoiceNumber,
    InvoiceStatus Status,
    DateOnly IssueDate,
    DateOnly DueDate,
    string? TraineeName,
    int BranchId,
    string BranchName,
    string Currency,
    decimal SubTotal,
    decimal DiscountTotal,
    decimal TaxTotal,
    decimal GrandTotal,
    decimal AmountPaid,
    decimal Outstanding,
    List<InvoiceLineDto> Lines);

public record OutstandingInvoiceDto(
    int InvoiceId,
    string InvoiceNumber,
    string? TraineeName,
    int BranchId,
    string BranchName,
    DateOnly DueDate,
    decimal GrandTotal,
    decimal AmountPaid,
    decimal Outstanding,
    bool IsOverdue);
