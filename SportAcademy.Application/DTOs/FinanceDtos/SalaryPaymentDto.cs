using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.FinanceDtos;

public record SalaryPaymentDto(
    int Id,
    int EmployeeId,
    string EmployeeName,
    int BranchId,
    string BranchName,
    decimal Amount,
    decimal Bonus,
    string Currency,
    DateOnly PeriodMonth,
    SalaryPaymentStatus Status,
    int? PaymentTypeId,
    string? PaymentTypeName,
    string? Notes,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    Guid? ReviewedByUserId,
    string? ReviewedByName,
    DateTime? ReviewedAt,
    string? RejectionReason,
    Guid? PaidByUserId,
    string? PaidByName,
    DateTime? PaidAt);

// Every active Employee left-joined to Coach for the payroll roster picker - IsCoach/CoachId
// are null when the employee has no coach record.
public record PayrollEmployeeDto(
    int EmployeeId,
    string FullName,
    int BranchId,
    string BranchName,
    string Position,
    decimal Salary,
    bool IsCoach,
    int? CoachId);
