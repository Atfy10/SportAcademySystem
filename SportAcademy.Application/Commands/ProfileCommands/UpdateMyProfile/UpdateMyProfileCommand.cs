using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.ProfileCommands.UpdateMyProfile;

// Partial-update pattern (null = leave unchanged), same convention as UpdateTenantProfileCommand
// - a caller only sends the field(s) they're actually changing. PhoneNumber is AppUser's own
// Identity field (account-level contact info, e.g. for a user with no linked Employee/Trainee
// record at all - Owner/Admin/Accountant), never Person.PhoneNumber on an Employee/Trainee,
// which is a separate, admin-managed field with its own uniqueness rule.
public record UpdateMyProfileCommand(string? PhoneNumber, string? ProfileImageUrl, string? Bio)
    : IRequest<Result<MeResponse>>, IRequiresFeature
{
    public string FeatureKey => "profile-mgmt";
}
