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
    private readonly ITenantStatusCache _tenantStatusCache;
    private readonly string _operation = OperationType.Get.ToString();

    public GetMyPermissionsQueryHandler(
        IUserContextService userContext,
        IUserRepository userRepository,
        IPermissionResolver permissionResolver,
        ITenantStatusCache tenantStatusCache)
    {
        _userContext = userContext;
        _userRepository = userRepository;
        _permissionResolver = permissionResolver;
        _tenantStatusCache = tenantStatusCache;
    }

    public async Task<Result<MyPermissionsDto>> Handle(GetMyPermissionsQuery request, CancellationToken ct)
    {
        var userId = _userContext.UserId
            ?? throw new IdNotFoundException(nameof(AppUser), Guid.Empty);

        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new IdNotFoundException(nameof(AppUser), userId);

        var roles = await _userRepository.GetUserRoleAsync(user, ct);
        var permissions = await _permissionResolver.GetEffectivePermissionsAsync(userId, ct);

        // TenantId always has a value here - TenantResolutionMiddleware's post-auth block
        // already 400s any authenticated request with no tenant claim, before this handler runs.
        var tenantId = _userContext.TenantId!.Value;
        var tenantStatus = await _tenantStatusCache.GetStatusAsync(tenantId, ct) ?? TenantStatus.Archived;

        return Result<MyPermissionsDto>.Success(
            new MyPermissionsDto(roles.ToList(), permissions.ToList(), !user.IsBanned, tenantStatus.ToString()),
            _operation);
    }
}
