using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Queries.AuthQueries.GetAllRoles;

public record GetAllRolesQuery : IRequest<Result<List<string>>>;
