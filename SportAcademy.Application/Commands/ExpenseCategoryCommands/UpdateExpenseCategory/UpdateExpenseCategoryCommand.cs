using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.ExpenseDtos;

namespace SportAcademy.Application.Commands.ExpenseCategoryCommands.UpdateExpenseCategory
{
    public record UpdateExpenseCategoryCommand(
        int Id,
        string? Name,
        bool? IsActive
    ) : IRequest<Result<ExpenseCategoryDto>>;
}
