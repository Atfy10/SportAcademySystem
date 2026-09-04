using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.ExpenseDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.ExpenseExceptions;

namespace SportAcademy.Application.Queries.ExpenseQueries.GetExpenseById;

public class GetExpenseByIdQueryHandler : IRequestHandler<GetExpenseByIdQuery, Result<ExpenseDto>>
{
    private readonly IExpenseRepository _repository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetExpenseByIdQueryHandler(IExpenseRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ExpenseDto>> Handle(GetExpenseByIdQuery request, CancellationToken ct)
    {
        var entity = await _repository.GetByIdWithIncludesAsync(request.Id, ct)
            ?? throw new ExpenseNotFoundException(request.Id.ToString());

        return Result<ExpenseDto>.Success(ExpenseMapper.ToDto(entity), _operation);
    }
}
