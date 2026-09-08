using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos.AdminDtos;

namespace SportAcademy.Application.Commands.AuthCommands.AdminCreateUser;

// Role is required (unlike an invitation's OwnerSetup/StaffOnboarding split) - this endpoint
// only ever creates staff directly, never an Owner (unique per tenant, set once at invitation
// acceptance) or a SuperAdmin (platform-only). BranchIds is only meaningful (and required) when
// Role is "Employee" - see AdminCreateUserCommandHandler.AssignableRoles.
public record AdminCreateUserCommand(
    string UserName,
    string Email,
    string? PhoneNumber,
    string Role,
    bool EmailConfirmed = false,
    bool IsActive = true,
    List<int>? BranchIds = null
) : IRequest<Result<AdminCreateUserResultDto>>;
