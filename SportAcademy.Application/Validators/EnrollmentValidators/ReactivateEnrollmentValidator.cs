using FluentValidation;
using SportAcademy.Application.Commands.EnrollmentCommands.ReactivateEnrollment;

namespace SportAcademy.Application.Validators.EnrollmentValidators
{
    public class ReactivateEnrollmentValidator : AbstractValidator<ReactivateEnrollmentCommand>
    {
        public ReactivateEnrollmentValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Please provide an enrollment ID.")
                .GreaterThan(0).WithMessage("Please provide a valid enrollment ID.");

            // The upper bound (can't exceed SessionAllowed) needs the enrollment loaded, so it's
            // checked in the handler instead - nothing here can see that value.
            RuleFor(x => x.SessionRemaining)
                .GreaterThanOrEqualTo(0).WithMessage("Sessions remaining can't be negative.");
        }
    }
}
