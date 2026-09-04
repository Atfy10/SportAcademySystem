using FluentValidation;
using SportAcademy.Application.Commands.SalaryPaymentCommands.RejectSalaryPayment;

namespace SportAcademy.Application.Validators.SalaryPaymentValidators
{
    public class RejectSalaryPaymentValidator : AbstractValidator<RejectSalaryPaymentCommand>
    {
        public RejectSalaryPaymentValidator()
        {
            RuleFor(x => x.Id).ApplyIdRuleFor("Salary payment");

            RuleFor(x => x.RejectionReason)
                .NotEmpty().WithMessage("A rejection reason is required.")
                .MaximumLength(500).WithMessage("Rejection reason must not exceed 500 characters.");
        }
    }
}
