using FluentValidation;
using SportAcademy.Application.Commands.SessionOccurrenceCommands.CreateSessionOccurrence;

namespace SportAcademy.Application.Validators.SessionOccurrenceValidators
{
    public class CreateSessionOccurrenceValidator : AbstractValidator<CreateSessionOccurrenceCommand>
    {
        public CreateSessionOccurrenceValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.GroupScheduleId)
                .NotEmpty().WithMessage("Please select a group schedule.")
                .GreaterThan(0).WithMessage("Group schedule ID must be a valid positive number.");

            RuleFor(x => x.StartDateTime)
                .NotEmpty().WithMessage("Please enter a start date and time.")
                .Must(start => start >= DateTime.Now.AddMinutes(-30))
                .WithMessage("Start date and time cannot be more than 30 minutes in the past.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Please select a session status.")
                .IsInEnum().WithMessage("Invalid session status selected. Please choose from the available options.");
        }
    }
}
