using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.UserCommands.UpdateUserPermissions
{
    public record PermissionOverrideInput(string Permission, PermissionEffect Effect);

    // Full replace of the user's Allow/Deny overrides (not additive) - mirrors
    // AssignRolesToUserCommand's replace-the-set semantics for roles. A permission simply
    // absent from `Overrides` inherits its role default; it is not implicitly denied.
    public record UpdateUserPermissionsCommand(Guid UserId, List<PermissionOverrideInput> Overrides)
        : IRequest<Result<bool>>;
}
