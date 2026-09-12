using FluentValidation;

namespace SportAcademy.Application.Commands.MarketingCommands.CreateLead
{
    public class CreateLeadCommandValidator : AbstractValidator<CreateLeadCommand>
    {
        public CreateLeadCommandValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
            RuleFor(x => x.AcademyName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
            // Digits only, country code included but no leading "+" (matches the marketing
            // form's own hint/pattern) - e.g. "201096042061", not "+201096042061" or
            // "01096042061". 8-15 digits covers the real range of country code + national
            // number lengths (E.164's own digit-count ceiling is 15).
            RuleFor(x => x.PhoneNumber).NotEmpty().Matches(@"^[0-9]{8,15}$")
                .WithMessage("Phone number must be digits only, including the country code, with no leading '+'.");
            RuleFor(x => x.City).MaximumLength(100);
            RuleFor(x => x.Message).MaximumLength(2000);
            RuleFor(x => x.Locale).NotEmpty().MaximumLength(5);
            RuleFor(x => x.BranchCount).GreaterThanOrEqualTo(0).When(x => x.BranchCount.HasValue);
            RuleFor(x => x.TraineeCountBand).GreaterThanOrEqualTo(0).When(x => x.TraineeCountBand.HasValue);
        }
    }
}
