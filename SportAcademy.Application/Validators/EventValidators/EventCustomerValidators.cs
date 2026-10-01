using FluentValidation;
using SportAcademy.Application.Commands.EventCustomerCommands.CreateEventCustomer;
using SportAcademy.Application.Commands.EventCustomerCommands.UpdateEventCustomer;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;

namespace SportAcademy.Application.Validators.EventValidators
{
    public class CreateEventCustomerValidator : AbstractValidator<CreateEventCustomerCommand>
    {
        public CreateEventCustomerValidator(
            IRegionalValidationService regionalValidation, ITenantSettingsCountryReader countryReader)
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Customer name is required.")
                .MaximumLength(200).WithMessage("Customer name must not exceed 200 characters.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .ApplyPhoneRuleFor(regionalValidation, countryReader);

            RuleFor(x => x.NationalityCategoryId).ApplyIdRuleFor("Nationality category");

            RuleFor(x => x.Notes).MaximumLength(1000);
        }
    }

    public class UpdateEventCustomerValidator : AbstractValidator<UpdateEventCustomerCommand>
    {
        public UpdateEventCustomerValidator(
            IRegionalValidationService regionalValidation, ITenantSettingsCountryReader countryReader)
        {
            RuleFor(x => x.Id).ApplyIdRuleFor("Customer");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Customer name is required.")
                .MaximumLength(200).WithMessage("Customer name must not exceed 200 characters.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .ApplyPhoneRuleFor(regionalValidation, countryReader);

            RuleFor(x => x.NationalityCategoryId).ApplyIdRuleFor("Nationality category");

            RuleFor(x => x.Notes).MaximumLength(1000);
        }
    }
}
