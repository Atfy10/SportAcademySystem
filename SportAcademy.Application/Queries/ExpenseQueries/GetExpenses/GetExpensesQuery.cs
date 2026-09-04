using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.ExpenseDtos;

namespace SportAcademy.Application.Queries.ExpenseQueries.GetExpenses;

public record GetExpensesQuery(
    PageRequest Page, int? BranchId, int? ExpenseCategoryId, DateOnly? From, DateOnly? To
) : IRequest<Result<PagedData<ExpenseDto>>>;
