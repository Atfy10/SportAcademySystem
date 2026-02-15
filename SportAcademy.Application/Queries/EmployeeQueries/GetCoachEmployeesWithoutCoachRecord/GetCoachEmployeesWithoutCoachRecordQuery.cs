using MediatR;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.Services;

namespace SportAcademy.Application.Queries.EmployeeQueries.GetCoachEmployeesWithoutCoachRecord
{
    public record GetCoachEmployeesWithoutCoachRecordQuery() : IRequest<Result<List<EmployeeDto>>>;
}
