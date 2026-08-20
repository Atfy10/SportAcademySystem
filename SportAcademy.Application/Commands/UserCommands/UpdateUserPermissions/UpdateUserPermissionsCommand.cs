using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.UserCommands.UpdateUserPermissions
{
    // Replaces the user's full set of individual permission grants (not additive) - mirrors
    // AssignRolesToUserCommand's replace-the-set semantics for roles.
    public record UpdateUserPermissionsCommand(Guid UserId, List<string> Permissions) : IRequest<Result<bool>>;
}
