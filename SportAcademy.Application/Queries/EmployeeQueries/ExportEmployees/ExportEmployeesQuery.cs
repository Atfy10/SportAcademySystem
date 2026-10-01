using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.EmployeeQueries.ExportEmployees;

// Ids empty or null = every employee the caller can see.
public record ExportEmployeesQuery(List<int>? Ids) : IRequest<Result<List<EmployeeExportDto>>>, IRequiresFeature
{
    public string FeatureKey => "employee-management";
}

public class ExportEmployeesQueryHandler
    : IRequestHandler<ExportEmployeesQuery, Result<List<EmployeeExportDto>>>
{
    private readonly IEmployeeRepository _employeeRepository;

    public ExportEmployeesQueryHandler(IEmployeeRepository employeeRepository)
        => _employeeRepository = employeeRepository;

    public async Task<Result<List<EmployeeExportDto>>> Handle(ExportEmployeesQuery request, CancellationToken cancellationToken)
    {
        var data = await _employeeRepository.GetExportDataAsync(request.Ids, cancellationToken);
        return Result<List<EmployeeExportDto>>.Success(data, "ExportEmployees");
    }
}
