namespace SportAcademy.Application.DTOs.ExpenseDtos;

public record ExpenseDto(
    int Id,
    string Title,
    int ExpenseCategoryId,
    string ExpenseCategoryName,
    int BranchId,
    string BranchName,
    decimal Amount,
    string Currency,
    DateOnly ExpenseDate,
    int? PaymentTypeId,
    string? PaymentTypeName,
    string? Notes,
    Guid RecordedByUserId,
    DateTime CreatedAt);
