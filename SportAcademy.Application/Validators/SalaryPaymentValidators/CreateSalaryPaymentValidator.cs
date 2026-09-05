using FluentValidation;
using SportAcademy.Application.Commands.SalaryPaymentCommands.CreateSalaryPayment;

namespace SportAcademy.Application.Validators.SalaryPaymentValidators
{
    public class CreateSalaryPaymentValidator : AbstractValidator<CreateSalaryPaymentCommand>
    {
        public CreateSalaryPaymentValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.EmployeeId).ApplyIdRuleFor("Employee");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");

            RuleFor(x => x.Bonus)
                .GreaterThanOrEqualTo(0).WithMessage("Bonus can't be negative.")
                .When(x => x.Bonus.HasValue);

            RuleFor(x => x.BranchId).ApplyOptionalIdRuleFor("Branch");

            RuleFor(x => x.PeriodMonth)
                .NotEqual(default(DateOnly)).WithMessage("A pay period is required.");

            RuleFor(x => x.PaymentTypeId).ApplyOptionalIdRuleFor("Payment type");
        }
    }
}
