using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Queries.EmployeeQueries.GetEmployeesCount;

public record GetEmployeesCountQuery() : IRequest<Result<int>>;
