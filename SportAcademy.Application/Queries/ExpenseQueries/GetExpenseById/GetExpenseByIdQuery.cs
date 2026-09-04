using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.ExpenseDtos;

namespace SportAcademy.Application.Queries.ExpenseQueries.GetExpenseById;

public record GetExpenseByIdQuery(int Id) : IRequest<Result<ExpenseDto>>;
