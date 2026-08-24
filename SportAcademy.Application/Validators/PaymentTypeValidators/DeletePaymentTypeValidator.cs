using FluentValidation;
using SportAcademy.Application.Commands.PaymentTypeCommands.DeletePaymentType;

namespace SportAcademy.Application.Validators.PaymentTypeValidators
{
    public class DeletePaymentTypeValidator : AbstractValidator<DeletePaymentTypeCommand>
    {
        public DeletePaymentTypeValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id must be greater than 0.");
        }
    }
}
