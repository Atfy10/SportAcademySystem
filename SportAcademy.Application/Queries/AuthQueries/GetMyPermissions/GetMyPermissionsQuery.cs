using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.Auth;

namespace SportAcademy.Application.Queries.AuthQueries.GetMyPermissions;

public record GetMyPermissionsQuery : IRequest<Result<MyPermissionsDto>>;
