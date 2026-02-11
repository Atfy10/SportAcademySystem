using FluentValidation;
using SportAcademy.Application.Commands.EmployeeCommands.CreateEmployee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.EmployeeValidators
{
    public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeCommand>
    {
        public CreateEmployeeValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name can't exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name can't exceed 50 characters.");

            RuleFor(x => x.SSN)
                .NotEmpty().WithMessage("National ID (SSN) is required.")
                .Matches(@"^\d{10,14}$").WithMessage("SSN must be between 10 and 14 digits.");

            RuleFor(x => x.Email)
              .NotEmpty()
              .EmailAddress();

            RuleFor(x => x.Nationality)
              .NotEmpty();

            RuleFor(x => x.Salary)
                .GreaterThan(0).WithMessage("Salary must be greater than zero.")
                .LessThanOrEqualTo(100000).WithMessage("Salary seems unusually high, please double-check.");

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage("Invalid gender value.");

            RuleFor(x => x.BirthDate)
                .LessThan(DateOnly.FromDateTime(DateTime.Now.AddYears(-16)))
                .WithMessage("Employee must be at least 16 years old.");

            RuleFor(x => x.Street)
                .NotEmpty().WithMessage("Street is required.")
                .MaximumLength(100).WithMessage("Street can't exceed 100 characters.");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required.")
                .MaximumLength(50).WithMessage("City can't exceed 50 characters.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^(?:\+965)?[569]\d{7}$").WithMessage("Enter a valid Kuwaiti phone number (8 digits, starting with 5, 6, or 9).");

            RuleFor(x => x.SecondNumber)
                .Matches(@"^(?:\+965)?[569]\d{7}$")
                .When(x => !string.IsNullOrWhiteSpace(x.SecondNumber))
                .WithMessage("Enter a valid secondary Kuwaiti phone number.");

            RuleFor(x => x.Position)
                .IsInEnum().WithMessage("Invalid position value.");

            RuleFor(x => x.BranchId)
                .GreaterThan(0).WithMessage("Please select a valid branch.");
        }
    }
}
