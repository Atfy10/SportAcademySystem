using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos.AdminDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.AuthCommands.AdminCreateUser;

// Role is required (unlike an invitation's OwnerSetup/StaffOnboarding split) - this endpoint
// only ever creates staff directly, never an Owner (unique per tenant, set once at invitation
// acceptance) or a SuperAdmin (platform-only). BranchIds is only meaningful (and required) when
// Role is "Employee" - see AdminCreateUserCommandHandler.AssignableRoles.
//
// Gated on user-management, not role-management, even though Role is required here - the
// primary action is "create a user" (same bucket as CreateInvitationCommand), not "change an
// existing user's roles" (AssignRolesToUserCommand, gated on role-management).
public record AdminCreateUserCommand(
    string UserName,
    string Email,
    string? PhoneNumber,
    string Role,
    bool EmailConfirmed = false,
    bool IsActive = true,
    List<int>? BranchIds = null
) : IRequest<Result<AdminCreateUserResultDto>>, IRequiresFeature
{
    public string FeatureKey => "user-management";
}
