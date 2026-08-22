using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.Auth;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Queries.AuthQueries.GetMyPermissions;

public class GetMyPermissionsQueryHandler : IRequestHandler<GetMyPermissionsQuery, Result<MyPermissionsDto>>
{
    private readonly IUserContextService _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IPermissionResolver _permissionResolver;
    private readonly string _operation = OperationType.Get.ToString();

    public GetMyPermissionsQueryHandler(
        IUserContextService userContext,
        IUserRepository userRepository,
        IPermissionResolver permissionResolver)
    {
        _userContext = userContext;
        _userRepository = userRepository;
        _permissionResolver = permissionResolver;
    }

    public async Task<Result<MyPermissionsDto>> Handle(GetMyPermissionsQuery request, CancellationToken ct)
    {
        var userId = _userContext.UserId
            ?? throw new IdNotFoundException(nameof(AppUser), Guid.Empty);

        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new IdNotFoundException(nameof(AppUser), userId);

        var roles = await _userRepository.GetUserRoleAsync(user, ct);
        var permissions = await _permissionResolver.GetEffectivePermissionsAsync(userId, ct);

        return Result<MyPermissionsDto>.Success(
            new MyPermissionsDto(roles.ToList(), permissions.ToList()), _operation);
    }
}
