using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.ExpenseDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.ExpenseQueries.GetExpenses;

public class GetExpensesQueryHandler : IRequestHandler<GetExpensesQuery, Result<PagedData<ExpenseDto>>>
{
    private readonly IExpenseRepository _repository;
    private readonly string _operation = OperationType.GetAll.ToString();

    public GetExpensesQueryHandler(IExpenseRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedData<ExpenseDto>>> Handle(GetExpensesQuery request, CancellationToken ct)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            request.Page, request.BranchId, request.ExpenseCategoryId, request.From, request.To, ct);

        var dtos = items.Select(ExpenseMapper.ToDto).ToList();

        return Result<PagedData<ExpenseDto>>.Success(new PagedData<ExpenseDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page.Page,
            PageSize = request.Page.PageSize,
        }, _operation);
    }
}
