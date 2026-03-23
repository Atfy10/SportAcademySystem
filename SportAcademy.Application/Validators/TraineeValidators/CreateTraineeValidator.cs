using FluentValidation;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Domain.Helpers;

namespace SportAcademy.Application.Validators.TraineeValidators
{
    public class CreateTraineeValidator : AbstractValidator<CreateTraineeCommand>
    {
        public CreateTraineeValidator()
        {
            RuleFor(t => t.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .NoDigits()
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(t => t.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .NoDigits()
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(t => t.SSN)
                .NotEmpty().WithMessage("SSN is required.")
                .Length(12).WithMessage("SSN must be exactly 12 digits.")
                .Matches(@"^\d{12}$").WithMessage("SSN must contain only numeric digits.")
                .Must((cmd, ssn) => PersonValidationHelper.IsValidSSN(ssn, cmd.BirthDate))
                .WithMessage("SSN must start with birth date components (YYMMDD prefix matching birth year).")
                .When(t => !string.IsNullOrEmpty(t.SSN));

            RuleFor(t => t.BirthDate)
                .LessThan(DateOnly.FromDateTime(DateTime.UtcNow.Date))
                .WithMessage("Birth date must be in the past.");

            RuleFor(t => t.GuardianName)
                .MaximumLength(50).WithMessage("Guardian name cannot exceed 50 characters.")
                .When(t => !string.IsNullOrEmpty(t.GuardianName));

            RuleFor(t => t.ParentNumber)
                .MinimumLength(8).WithMessage("Parent phone number must be at least 8 characters.")
                .MaximumLength(13).WithMessage("Parent phone number cannot exceed 13 characters.")
                .When(t => !string.IsNullOrEmpty(t.ParentNumber));

            RuleFor(t => t.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .MinimumLength(8).WithMessage("Phone number must be at least 8 characters.")
                .MaximumLength(12).WithMessage("Phone number cannot exceed 12 characters.");

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
