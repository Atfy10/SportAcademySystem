using FluentValidation;
using SportAcademy.Application.Commands.ExcuseRequestCommands.CreateExcuseRequest;

namespace SportAcademy.Application.Validators.ExcuseRequestValidators
{
    public class CreateExcuseRequestValidator : AbstractValidator<CreateExcuseRequestCommand>
    {
        public CreateExcuseRequestValidator()
        {
            RuleFor(x => x.SessionOccurrenceId)
                .GreaterThan(0).WithMessage("Please select a valid session.");

            RuleFor(x => x.TraineeId)
                .GreaterThan(0).WithMessage("Please select a valid trainee.");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Please provide a reason for the excuse.")
                .MaximumLength(500).WithMessage("Reason can't exceed 500 characters.");
        }
    }
}
