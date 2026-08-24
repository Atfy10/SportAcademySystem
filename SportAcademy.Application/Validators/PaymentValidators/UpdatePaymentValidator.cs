using FluentValidation;
using SportAcademy.Application.Commands.PaymentCommands.UpdatePayment;

namespace SportAcademy.Application.Validators.PaymentValidators
{
    public class UpdatePaymentValidator : AbstractValidator<UpdatePaymentCommand>
    {
        public UpdatePaymentValidator()
        {
            RuleFor(p => p.PaymentNumber)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Payment number is required.");

            RuleFor(p => p.PaymentTypeId)
                .GreaterThan(0).WithMessage("A payment type must be selected.");

            RuleFor(p => p.PaidDate)
                .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
                .WithMessage("Paid date cannot be in the future.");
        }
    }
}
