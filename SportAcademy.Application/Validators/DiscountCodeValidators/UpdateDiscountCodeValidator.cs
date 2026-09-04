using FluentValidation;
using SportAcademy.Application.Commands.DiscountCodeCommands.UpdateDiscountCode;

namespace SportAcademy.Application.Validators.DiscountCodeValidators
{
    public class UpdateDiscountCodeValidator : AbstractValidator<UpdateDiscountCodeCommand>
    {
        public UpdateDiscountCodeValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Id).ApplyIdRuleFor("Discount code");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("A code is required.")
                .MaximumLength(30).WithMessage("Code must not exceed 30 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(200).WithMessage("Description must not exceed 200 characters.");

            RuleFor(x => x.PercentageOff)
                .GreaterThan(0).WithMessage("Percentage off must be greater than zero.")
                .LessThanOrEqualTo(100).WithMessage("Percentage off cannot exceed 100.");
        }
    }
}
