using FluentValidation;
using SportAcademy.Application.Commands.PaymentTypeCommands.UpdatePaymentType;

namespace SportAcademy.Application.Validators.PaymentTypeValidators
{
    public class UpdatePaymentTypeValidator : AbstractValidator<UpdatePaymentTypeCommand>
    {
        public UpdatePaymentTypeValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id must be greater than 0.");

            RuleFor(x => x.Name)
                .MaximumLength(50).WithMessage("Name must not exceed 50 characters.")
                .When(x => x.Name is not null);

            RuleFor(x => x.NameAr)
                .MaximumLength(100).WithMessage("Arabic name must not exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.NameAr));
        }
    }
}
