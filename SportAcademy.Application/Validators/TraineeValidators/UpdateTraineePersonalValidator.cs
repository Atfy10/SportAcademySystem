using FluentValidation;
using SportAcademy.Application.Commands.Trainees.UpdateTrainee;

namespace SportAcademy.Application.Validators.TraineeValidators
{
    public class UpdateTraineePersonalValidator : AbstractValidator<UpdateTraineePersonalCommand>
    {
        public UpdateTraineePersonalValidator()
        {
            RuleFor(t => t.FirstName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.")
                .When(t => !string.IsNullOrEmpty(t.GuardianName));


            RuleFor(t => t.LastName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.")
                .When(t => !string.IsNullOrEmpty(t.GuardianName));

            RuleFor(t => t.GuardianName)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(50).WithMessage("Guardian name cannot exceed 50 characters.")
                .When(t => !string.IsNullOrEmpty(t.GuardianName));

            RuleFor(t => t.ParentNumber)
                .Cascade(CascadeMode.Stop)
                .Length(8).WithMessage("Parent phone number must be exactly 8 characters.")
                .When(t => !string.IsNullOrEmpty(t.ParentNumber));
        }
    }
}
