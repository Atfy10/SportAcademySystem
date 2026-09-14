using FluentValidation;
using SportAcademy.Application.Commands.Trainees.UpdateTraineeAcademicInfo;

namespace SportAcademy.Application.Validators.TraineeValidators
{
    public class UpdateTraineeAcademicInfoValidator : AbstractValidator<UpdateTraineeAcademicInfoCommand>
    {
        public UpdateTraineeAcademicInfoValidator()
        {
            RuleFor(t => t.Id)
                .GreaterThan(0)
                .WithMessage("Invalid trainee ID.");

            RuleFor(t => t.BranchId)
                .GreaterThan(0)
                .WithMessage("Please select a valid branch.");

            RuleFor(t => t.SportIds)
                .NotNull()
                .WithMessage("Sports are required.");
        }
    }
}
