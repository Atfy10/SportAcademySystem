using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Commands.AuthCommands.AssignRolesToUser;

public class AssignRolesToUserCommandHandler : IRequestHandler<AssignRolesToUserCommand, Result<bool>>
{
    // Same restriction CreateInvitationCommandHandler applies to invitations - Owner is set
    // once per tenant at OwnerSetup acceptance and SuperAdmin is platform-only. Without this,
    // this generic endpoint (gated only on the ordinary "tenant.users.manage" permission an
    // Admin already holds) was a direct privilege-escalation path: any Admin could hand any
    // tenant user - including themselves - the Owner role, or even the platform-wide
    // SuperAdmin role, bypassing that restriction entirely.
    private static readonly string[] RestrictedRoles = ["Owner", "SuperAdmin"];

    private readonly IUserRepository _userRepository;
    private readonly IPermissionCacheInvalidator _cacheInvalidator;
    private readonly string _operation = OperationType.Update.ToString();

    public AssignRolesToUserCommandHandler(IUserRepository userRepository, IPermissionCacheInvalidator cacheInvalidator)
    {
        _userRepository = userRepository;
        _cacheInvalidator = cacheInvalidator;
    }

    public async Task<Result<bool>> Handle(AssignRolesToUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new IdNotFoundException(nameof(AppUser), request.UserId);

        // Only block *newly granting* a restricted role - a user who already holds Owner/
        // SuperAdmin can still have their other roles edited (or be re-saved unchanged)
        // without this generic endpoint being the thing that revokes a platform-level role.
        var currentRoles = await _userRepository.GetUserRoleAsync(user, cancellationToken);
        var newlyGrantedRestricted = request.Roles
            .Where(r => RestrictedRoles.Contains(r, StringComparer.OrdinalIgnoreCase))
            .Where(r => !currentRoles.Any(cr => string.Equals(cr, r, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        if (newlyGrantedRestricted.Count > 0)
            return Result<bool>.Failure(
                _operation,
                $"'{string.Join(", ", newlyGrantedRestricted)}' cannot be granted through role management - " +
                "Owner is set once at tenant setup and SuperAdmin is platform-only.",
                400);

        var (result, notFoundRoles) = await _userRepository.ReplaceRolesAsync(user, request.Roles, cancellationToken);

        if (!result.Succeeded)
        {
            var errors = result.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

            return Result<bool>.Failure(_operation, "Failed to assign roles.", 400, errors);
        }

        // A role change directly changes the resolved permission set - without this, a
        // just-demoted user would keep their old effective permissions for up to the cache's
        // sliding window.
        _cacheInvalidator.Invalidate(request.UserId);

        return Result<bool>.Success(true, _operation);
    }
}
