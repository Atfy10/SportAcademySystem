using MediatR;
using Microsoft.AspNetCore.Identity;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Authorization;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Queries.UserQueries.GetUserPermissions;

public class GetUserPermissionsQueryHandler : IRequestHandler<GetUserPermissionsQuery, Result<List<UserPermissionStatusDto>>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly IUserPermissionOverrideRepository _overrideRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetUserPermissionsQueryHandler(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        IUserPermissionOverrideRepository overrideRepository)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _overrideRepository = overrideRepository;
    }

    public async Task<Result<List<UserPermissionStatusDto>>> Handle(
        GetUserPermissionsQuery request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString())
            ?? throw new IdNotFoundException(nameof(AppUser), request.UserId);

        var roles = await _userManager.GetRolesAsync(user);

        var roleDefaults = new HashSet<string>();
        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is null) continue;
            var claims = await _roleManager.GetClaimsAsync(role);
            foreach (var claim in claims.Where(c => c.Type == "permission"))
                roleDefaults.Add(claim.Value);
        }

        var overrides = (await _overrideRepository.GetForUserAsync(user.Id, ct))
            .ToDictionary(o => o.Permission, o => o.Effect);

        var rows = Permissions.All
            .Where(p => !p.StartsWith("platform."))
            .Select(p =>
            {
                var roleDefault = roleDefaults.Contains(p);
                var hasOverride = overrides.TryGetValue(p, out var effect);
                var effective = hasOverride ? effect == PermissionEffect.Allow : roleDefault;
                return new UserPermissionStatusDto(p, roleDefault, hasOverride ? effect : null, effective);
            })
            .ToList();

        return Result<List<UserPermissionStatusDto>>.Success(rows, _operation);
    }
}
