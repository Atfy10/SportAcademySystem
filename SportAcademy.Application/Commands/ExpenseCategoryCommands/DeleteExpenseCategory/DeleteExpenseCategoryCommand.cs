using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.ExpenseCategoryCommands.DeleteExpenseCategory
{
    public record DeleteExpenseCategoryCommand(int Id) : IRequest<Result<bool>>;
}
