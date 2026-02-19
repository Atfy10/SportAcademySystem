using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EmployeeDtos;

namespace SportAcademy.Application.Queries.EmployeeQueries.GetCoachEmployeesWithoutCoachRecord
{
    public record GetCoachEmployeesWithoutCoachRecordQuery() : IRequest<Result<List<EmployeeDto>>>;
}
