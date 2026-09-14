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

            // Both fields are optional (nullable) on the command by design - a caller updating
            // only the status (e.g. SessionOccurrencesNearbyModal's Complete/Cancel action, which
            // never sends StartDateTime at all) must not be rejected just because the other
            // optional field was omitted.
            RuleFor(x => x.StartDateTime)
                .Must(start => start >= DateTime.UtcNow.AddMinutes(-30))
                .WithMessage("Start time cannot be more than 30 minutes in the past.")
                .When(x => x.StartDateTime.HasValue);

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid session status selected. Please choose from the available options.")
                .When(x => x.Status.HasValue);
        }
    }
}
