using FluentValidation;
using SportAcademy.Application.Commands.EmployeeCommands.UpdateEmployee;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;

namespace SportAcademy.Application.Validators.EmployeeValidators
{
    public class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeCommand>
    {
        public UpdateEmployeeValidator(
            IRegionalValidationService regionalValidation,
            ITenantSettingsCountryReader countryReader)
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Invalid employee ID.");

            // UpdateEmployeeCommand is a partial-update contract - every field but Id is optional
            // and left untouched when the caller doesn't send it (see the command's own doc
            // comment and EmployeeMapper.ApplyUpdate). Every rule below must therefore only run
            // when the field is actually present, or a legitimate partial update (e.g. the
            // employee-edit form, which never sends FirstName/LastName at all since those are
            // read-only there) gets rejected outright even though nothing about it is invalid.
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .NoDigits()
                .MaximumLength(50).WithMessage("First name can't exceed 50 characters.")
                .When(x => x.FirstName != null);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .NoDigits()
                .MaximumLength(50).WithMessage("Last name can't exceed 50 characters.")
                .When(x => x.LastName != null);

            RuleFor(x => x.Salary)
                .GreaterThan(0).WithMessage("Salary must be greater than zero.")
                .LessThanOrEqualTo(100000).WithMessage("Salary seems unusually high, please double-check.")
                .When(x => x.Salary.HasValue);

            RuleFor(x => x.Street)
                .NotEmpty().WithMessage("Street is required.")
                .MaximumLength(100).WithMessage("Street can't exceed 100 characters.")
                .When(x => x.Street != null);

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required.")
                .MaximumLength(50).WithMessage("City can't exceed 50 characters.")
                .When(x => x.City != null);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .ApplyPhoneRuleFor(regionalValidation, countryReader)
                .When(x => x.PhoneNumber != null);

            RuleFor(x => x.SecondPhoneNumber)
                .ApplyPhoneRuleFor(regionalValidation, countryReader)
                .When(x => !string.IsNullOrWhiteSpace(x.SecondPhoneNumber))
                .WithMessage("Secondary phone number is not valid for the configured region.");

            RuleFor(x => x.Position)
                .IsInEnum().WithMessage("Invalid position value.")
                .When(x => x.Position.HasValue);

            RuleFor(x => x.BranchId)
                .GreaterThan(0)
                .WithMessage("Please select a valid branch.")
                .When(x => x.BranchId.HasValue);
        }
    }
}
