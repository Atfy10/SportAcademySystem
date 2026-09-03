using FluentValidation;
using SportAcademy.Application.Commands.TraineeGroupCommands.PauseTraineeGroup;

namespace SportAcademy.Application.Validators.TraineeGroupValidators
{
    public class PauseTraineeGroupValidator : AbstractValidator<PauseTraineeGroupCommand>
    {
        public PauseTraineeGroupValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Trainee group ID must be a valid positive number.");

            RuleFor(x => x.Reason)
                .MaximumLength(500).WithMessage("Pause reason can't exceed 500 characters.");
        }
    }
}
