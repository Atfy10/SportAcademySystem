using FluentValidation;
using SportAcademy.Application.Commands.FinanceCommands.RefundPayment;
using SportAcademy.Application.Commands.FinanceCommands.VoidPayment;

namespace SportAcademy.Application.Validators.FinanceValidators
{
    // The upper bound (the refundable balance) needs the payment itself, so the ledger enforces
    // it (FinanceRuleException.RefundOutOfRange); this only rejects what's wrong on its face.
    public class RefundPaymentValidator : AbstractValidator<RefundPaymentCommand>
    {
        public RefundPaymentValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.PaymentNumber)
                .NotEmpty().WithMessage("Payment number is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Refund amount must be greater than zero.")
                .Must(a => decimal.Round(a, 3) == a).WithMessage("Refund amount can have at most 3 decimal places.");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Please give a reason for the refund.")
                .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");

            RuleFor(x => x.NewDueDate)
                .Must(d => d is null || d >= DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("The new due date can't be in the past.");
        }
    }

    public class VoidPaymentValidator : AbstractValidator<VoidPaymentCommand>
    {
        public VoidPaymentValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.PaymentNumber)
                .NotEmpty().WithMessage("Payment number is required.");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Please give a reason for voiding this payment.")
                .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");

            RuleFor(x => x.NewDueDate)
                .Must(d => d is null || d >= DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("The new due date can't be in the past.");
        }
    }
}
