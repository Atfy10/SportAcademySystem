using FluentValidation;
using SportAcademy.Application.Commands.PaymentTypeCommands.CreatePaymentType;

namespace SportAcademy.Application.Validators.PaymentTypeValidators
{
    public class CreatePaymentTypeValidator : AbstractValidator<CreatePaymentTypeCommand>
    {
        public CreatePaymentTypeValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(50).WithMessage("Name must not exceed 50 characters.");

            RuleFor(x => x.NameAr)
                .MaximumLength(100).WithMessage("Arabic name must not exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.NameAr));
        }
    }
}
