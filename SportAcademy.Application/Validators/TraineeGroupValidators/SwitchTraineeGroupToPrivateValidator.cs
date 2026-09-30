using FluentValidation;
using SportAcademy.Application.Commands.TraineeGroupCommands.SwitchTraineeGroupToPrivate;

namespace SportAcademy.Application.Validators.TraineeGroupValidators
{
    public class SwitchTraineeGroupToPrivateValidator : AbstractValidator<SwitchTraineeGroupToPrivateCommand>
    {
        public SwitchTraineeGroupToPrivateValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Trainee group ID must be a valid positive number.");

            RuleFor(x => x.MaximumCapacity)
                .GreaterThan(0).WithMessage("Maximum capacity must be greater than 0.")
                .LessThanOrEqualTo(TraineeGroupCapacity.PrivateMaximum)
                .WithMessage($"A private group cannot exceed {TraineeGroupCapacity.PrivateMaximum} trainees.");

            RuleFor(x => x.KeepTraineeIds)
                .NotNull().WithMessage("Please choose which trainees continue in the group.");

            RuleFor(x => x)
                .Must(x => x.KeepTraineeIds.Distinct().Count() <= x.MaximumCapacity)
                .WithMessage("More trainees are selected than the new capacity allows.")
                .When(x => x.KeepTraineeIds is not null);
        }
    }
}
