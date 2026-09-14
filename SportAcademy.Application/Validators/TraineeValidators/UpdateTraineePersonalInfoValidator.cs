using FluentValidation;
using SportAcademy.Application.Commands.Trainees.UpdateTraineePersonalInfo;

namespace SportAcademy.Application.Validators.TraineeValidators
{
    public class UpdateTraineePersonalInfoValidator : AbstractValidator<UpdateTraineePersonalInfoCommand>
    {
        public UpdateTraineePersonalInfoValidator()
        {
            RuleFor(t => t.Id)
                .GreaterThan(0)
                .WithMessage("Invalid trainee ID.");

            // True partial update - every rule below only runs when the field is actually
            // present, so a caller that omits it (e.g. this form not touching FirstName) never
            // gets rejected. Same pattern as UpdateEmployeeValidator.
            RuleFor(t => t.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .NoDigits()
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.")
                .When(t => t.FirstName != null);

            RuleFor(t => t.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .NoDigits()
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.")
                .When(t => t.LastName != null);

            RuleFor(t => t.GuardianName)
                .MaximumLength(50).WithMessage("Guardian name cannot exceed 50 characters.")
                .When(t => !string.IsNullOrEmpty(t.GuardianName));

            RuleFor(t => t.ParentNumber)
                .Length(8).WithMessage("Parent phone number must be exactly 8 characters.")
                .When(t => !string.IsNullOrEmpty(t.ParentNumber));
        }
    }
}
