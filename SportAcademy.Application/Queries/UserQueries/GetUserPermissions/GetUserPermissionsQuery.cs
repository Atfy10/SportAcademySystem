using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos;

namespace SportAcademy.Application.Queries.UserQueries.GetUserPermissions;

public record GetUserPermissionsQuery(Guid UserId) : IRequest<Result<List<UserPermissionStatusDto>>>;
