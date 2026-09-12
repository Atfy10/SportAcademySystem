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
            RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(30);
            RuleFor(x => x.City).MaximumLength(100);
            RuleFor(x => x.Message).MaximumLength(2000);
            RuleFor(x => x.Locale).NotEmpty().MaximumLength(5);
            RuleFor(x => x.BranchCount).GreaterThanOrEqualTo(0).When(x => x.BranchCount.HasValue);
            RuleFor(x => x.TraineeCountBand).GreaterThanOrEqualTo(0).When(x => x.TraineeCountBand.HasValue);
        }
    }
}
