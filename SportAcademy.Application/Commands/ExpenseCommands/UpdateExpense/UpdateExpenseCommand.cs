using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.ExpenseDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.ExpenseCommands.UpdateExpense
{
    public record UpdateExpenseCommand(
        int Id,
        string? Title,
        int? ExpenseCategoryId,
        int? BranchId,
        decimal? Amount,
        DateOnly? ExpenseDate,
        int? PaymentTypeId,
        string? Notes
    ) : IRequest<Result<ExpenseDto>>, IOptionallyBranchScopedRequest;
}
