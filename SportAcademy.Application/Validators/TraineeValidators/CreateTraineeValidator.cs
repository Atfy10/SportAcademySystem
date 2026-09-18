using FluentValidation;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;

namespace SportAcademy.Application.Validators.TraineeValidators
{
    public class CreateTraineeValidator : AbstractValidator<CreateTraineeCommand>
    {
        public CreateTraineeValidator(
            IRegionalValidationService regionalValidation,
            ITenantSettingsCountryReader countryReader)
        {
            RuleFor(t => t.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .NoDigits()
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(t => t.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .NoDigits()
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

            // SSN is optional - ApplyNationalIdRuleFor itself treats empty as valid, so no
            // separate .When() gate is needed the way the old length/regex chain needed one.
            RuleFor(t => t.SSN)
                .ApplyNationalIdRuleFor(regionalValidation, countryReader, cmd => cmd.BirthDate)
                .WithMessage("SSN is not a valid National ID for the configured region.");

            RuleFor(t => t.BirthDate)
                .LessThan(DateOnly.FromDateTime(DateTime.UtcNow.Date))
                .WithMessage("Birth date must be in the past.");

            RuleFor(t => t.GuardianName)
                .MaximumLength(50).WithMessage("Guardian name cannot exceed 50 characters.")
                .When(t => !string.IsNullOrEmpty(t.GuardianName));

            RuleFor(t => t.ParentNumber)
                .ApplyPhoneRuleFor(regionalValidation, countryReader)
                .WithMessage("Parent phone number is not valid for the configured region.")
                .When(t => !string.IsNullOrEmpty(t.ParentNumber));

            RuleFor(t => t.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .ApplyPhoneRuleFor(regionalValidation, countryReader);

            RuleFor(t => t.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email address is invalid.");

            RuleFor(t => t.Nationality)
                .IsInEnum().WithMessage("Nationality is required.");

            RuleFor(t => t.Street)
                .MaximumLength(70).WithMessage("Street address cannot exceed 70 characters.");

            RuleFor(t => t.City)
                .MaximumLength(50).WithMessage("City cannot exceed 50 characters.");

            RuleFor(t => t.BranchId)
                .ApplyIdRuleFor("Branch");

            RuleFor(t => t.NationalityCategoryId)
                .ApplyIdRuleFor("NationalityCategory");

            RuleFor(t => t.FamilyId)
                .GreaterThanOrEqualTo(0).WithMessage("Family ID must be a valid number.");

            RuleFor(t => t.Gender)
                .IsInEnum().WithMessage("Gender is required.");
        }
    }
}
