using FluentValidation;

namespace SportAcademy.Application.Commands.ProfileCommands.UpdateMyProfile;

public class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        // Same pattern CreateUserValidator applies to this same field (AppUser.PhoneNumber) -
        // only runs when a value was actually sent, since null here means "leave unchanged".
        // Empty string is distinct from null: it's the frontend's explicit "clear this field"
        // value (MyProfile.tsx never sends null for a field it renders), so it must bypass the
        // format check rather than be rejected as an invalid phone number.
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^(\+965)?[2569]\d{7}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Please enter a valid Kuwait phone number (e.g., +96551234567).");

        RuleFor(x => x.Bio)
            .MaximumLength(1000)
            .WithMessage("Bio cannot exceed 1000 characters.");

        RuleFor(x => x.ProfileImageUrl)
            .MaximumLength(500);
    }
}
