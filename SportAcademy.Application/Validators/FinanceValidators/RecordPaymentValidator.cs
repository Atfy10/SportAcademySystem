using FluentValidation;
using SportAcademy.Application.Commands.FinanceCommands.RecordPayment;

namespace SportAcademy.Application.Validators.FinanceValidators
{
    public class RecordPaymentValidator : AbstractValidator<RecordPaymentCommand>
    {
        public RecordPaymentValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Payment amount must be greater than zero.");

            RuleFor(x => x.BranchId)
                .ApplyIdRuleFor("Branch");

            RuleFor(x => x.Allocations)
                .NotEmpty().WithMessage("Select at least one invoice to apply this payment to.");

            RuleForEach(x => x.Allocations).ChildRules(alloc =>
            {
                alloc.RuleFor(a => a.InvoiceId).ApplyIdRuleFor("Invoice");
                alloc.RuleFor(a => a.Amount).GreaterThan(0).WithMessage("Each allocation amount must be greater than zero.");
            });

            RuleFor(x => x)
                .Must(x => x.Allocations.Sum(a => a.Amount) == x.Amount)
                .WithMessage("Allocations must sum exactly to the payment amount.");
        }
    }
}
