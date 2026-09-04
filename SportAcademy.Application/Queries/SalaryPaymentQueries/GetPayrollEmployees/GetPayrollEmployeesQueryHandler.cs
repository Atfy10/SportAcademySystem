using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.SalaryPaymentQueries.GetPayrollEmployees;

public class GetPayrollEmployeesQueryHandler : IRequestHandler<GetPayrollEmployeesQuery, Result<List<PayrollEmployeeDto>>>
{
    private readonly ISalaryPaymentRepository _repository;
    private readonly string _operation = OperationType.GetAll.ToString();

    public GetPayrollEmployeesQueryHandler(ISalaryPaymentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<PayrollEmployeeDto>>> Handle(GetPayrollEmployeesQuery request, CancellationToken ct)
    {
        var rows = await _repository.GetPayrollEmployeesAsync(request.BranchId, request.Search, ct);

        return Result<List<PayrollEmployeeDto>>.Success(rows, _operation);
    }
}
