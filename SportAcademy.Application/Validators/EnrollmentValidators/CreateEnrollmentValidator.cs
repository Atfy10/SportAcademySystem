using FluentValidation;
using SportAcademy.Application.Commands.EnrollmentCommands.CreateEnrollment;

namespace SportAcademy.Application.Validators.EnrollmentValidators
{
    public class CreateEnrollmentValidator : AbstractValidator<CreateEnrollmentCommand>
    {
        public CreateEnrollmentValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.EnrollmentDate)
                .NotEmpty().WithMessage("Please provide an enrollment date.")
                .Must(x => x <= DateTime.Now.AddDays(1))
                .WithMessage("Enrollment date can't be set in the future.");

            RuleFor(x => x.ExpiryDate)
                .NotEmpty().WithMessage("Please provide an expiry date.")
                .GreaterThan(x => x.EnrollmentDate)
                .WithMessage("Expiry date should be after the enrollment date.");

            RuleFor(x => x.SessionAllowed)
                .NotEmpty().WithMessage("Please specify the number of sessions allowed.")
                .GreaterThan(0).WithMessage("At least 1 session must be allowed.")
                .LessThanOrEqualTo(24).WithMessage("Maximum 24 sessions can be allowed.");

            RuleFor(x => x.TraineeId)
                .NotEmpty().WithMessage("Please select a trainee.")
                .GreaterThan(0).WithMessage("Please select a valid trainee.");

            RuleFor(x => x.TraineeGroupId)
                .NotEmpty().WithMessage("Please select a trainee group.")
                .GreaterThan(0).WithMessage("Please select a valid trainee group.");
        }
    }
}
