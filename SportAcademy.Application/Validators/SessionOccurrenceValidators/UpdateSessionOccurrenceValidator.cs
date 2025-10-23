using FluentValidation;
using SportAcademy.Application.Commands.SessionOccurrenceCommands.UpdateSessionOccurrence;

namespace SportAcademy.Application.Validators.SessionOccurrenceValidators
{
    public class UpdateSessionOccurrenceValidator : AbstractValidator<UpdateSessionOccurrenceCommand>
    {
        public UpdateSessionOccurrenceValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Please provide the session occurrence ID.")
                .GreaterThan(0).WithMessage("Session occurrence ID must be a valid positive number.");

            RuleFor(x => x.StartDateTime)
                .NotEmpty().WithMessage("Please enter the session start date and time.")
                .Must(start => start >= DateTime.Now.AddMinutes(-30))
                .WithMessage("Start time cannot be more than 30 minutes in the past.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Please select a session status.")
                .IsInEnum().WithMessage("Invalid session status selected. Please choose from the available options.");
        }
    }
}
