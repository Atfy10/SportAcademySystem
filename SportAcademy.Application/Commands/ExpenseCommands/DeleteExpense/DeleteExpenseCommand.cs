using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.ExpenseCommands.DeleteExpense
{
    public record DeleteExpenseCommand(int Id) : IRequest<Result<bool>>;
}
