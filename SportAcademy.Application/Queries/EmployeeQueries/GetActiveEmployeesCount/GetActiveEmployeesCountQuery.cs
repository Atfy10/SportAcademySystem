using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Queries.EmployeeQueries.GetActiveEmployeesCount;

public record GetActiveEmployeesCountQuery() : IRequest<Result<int>>;
