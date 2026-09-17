using FluentValidation;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Validators;
using SportAcademy.Domain.Contract;

namespace SportAcademy.Application.Commands.ProfileCommands.UpdateMyProfile;

public class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator(
        IRegionalValidationService regionalValidation,
        ITenantSettingsCountryReader countryReader)
    {
        // Same pattern CreateUserValidator applies to this same field (AppUser.PhoneNumber) -
        // only runs when a value was actually sent, since null here means "leave unchanged".
        // Empty string is distinct from null: it's the frontend's explicit "clear this field"
        // value (MyProfile.tsx never sends null for a field it renders), so it must bypass the
        // format check rather than be rejected as an invalid phone number.
        RuleFor(x => x.PhoneNumber)
            .ApplyPhoneRuleFor(regionalValidation, countryReader)
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Bio)
            .MaximumLength(1000)
            .WithMessage("Bio cannot exceed 1000 characters.");

        RuleFor(x => x.ProfileImageUrl)
            .MaximumLength(500);
    }
}
