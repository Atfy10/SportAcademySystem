using FluentValidation;
using SportAcademy.Application.Commands.CoachCommands.RateCoach;

namespace SportAcademy.Application.Validators.CoachValidators;

public class RateCoachValidator : AbstractValidator<RateCoachCommand>
{
    public RateCoachValidator()
    {
        RuleFor(x => x.CoachId)
            .ApplyIdRuleFor("Coach");

        RuleFor(x => x.Rate)
            .InclusiveBetween(1, 5).WithMessage("Rate must be between 1 and 5.");
    }
}
