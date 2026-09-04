using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.ExpenseDtos;

namespace SportAcademy.Application.Commands.ExpenseCategoryCommands.CreateExpenseCategory
{
    public record CreateExpenseCategoryCommand(
        string Name,
        bool IsActive
    ) : IRequest<Result<ExpenseCategoryDto>>;
}
