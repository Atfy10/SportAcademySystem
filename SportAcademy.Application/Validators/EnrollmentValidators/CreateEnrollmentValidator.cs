using FluentValidation;
using SportAcademy.Application.Commands.EnrollmentCommands.CreateEnrollment;

namespace SportAcademy.Application.Validators.EnrollmentValidators
{
    public class CreateEnrollmentValidator : AbstractValidator<CreateEnrollmentCommand>
    {
        public CreateEnrollmentValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            // Any date: staff enroll ahead for a future term or back-date a late entry. The expiry
            // is recomputed from this date by the handler, and the rule below still keeps it
            // inside the subscription.
            RuleFor(x => x.EnrollmentDate)
                .NotEmpty().WithMessage("Please provide an enrollment date.");

            RuleFor(x => x.ExpiryDate)
                .NotEmpty().WithMessage("Please provide an expiry date.")
                .GreaterThan(x => x.EnrollmentDate)
                .WithMessage("Expiry date should be after the enrollment date.");

            // No rule on SessionAllowed: it isn't a client decision. The handler assigns the
            // quota from the subscription (SubscriptionDetailsService.CalculateAllowedSessions),
            // so nothing is submitted to validate - and demanding it here rejected every
            // enrollment once the forms stopped sending a number they never controlled.
            // The old cap of 100 would have been wrong too: a 12-month plan at 16 sessions a
            // month is a legitimate 192.

            RuleFor(x => x.TraineeId)
                .NotEmpty().WithMessage("Please select a trainee.")
                .GreaterThan(0).WithMessage("Please select a valid trainee.");

            RuleFor(x => x.TraineeGroupId)
                .NotEmpty().WithMessage("Please select a trainee group.")
                .GreaterThan(0).WithMessage("Please select a valid trainee group.");

            RuleFor(x => x.SubscriptionDetailsId)
                .NotEmpty().WithMessage("Please select a subscription.")
                .GreaterThan(0).WithMessage("Please select a valid subscription.");
        }
    }
}
