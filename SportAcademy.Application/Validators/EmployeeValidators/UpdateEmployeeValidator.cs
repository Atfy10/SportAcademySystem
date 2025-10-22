﻿using FluentValidation;
using SportAcademy.Application.Commands.EmployeeCommands.UpdateEmployee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.EmployeeValidators
{
    public class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeCommand>
    {
        public UpdateEmployeeValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Invalid employee ID.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name can't exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name can't exceed 50 characters.");

            RuleFor(x => x.Salary)
                .GreaterThan(0).WithMessage("Salary must be greater than zero.")
                .LessThanOrEqualTo(100000).WithMessage("Salary seems unusually high, please double-check.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required.")
                .MaximumLength(200).WithMessage("Address can't exceed 200 characters.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^(?:\+965)?[569]\d{7}$")
                .WithMessage("Enter a valid Kuwaiti phone number (8 digits, starting with 5, 6, or 9).");

            RuleFor(x => x.SecondPhoneNumber)
                .Matches(@"^(?:\+965)?[569]\d{7}$")
                .When(x => !string.IsNullOrWhiteSpace(x.SecondPhoneNumber))
                .WithMessage("Enter a valid secondary Kuwaiti phone number.");

            RuleFor(x => x.Position)
                .IsInEnum().WithMessage("Invalid position value.");

            RuleFor(x => x.BranchId)
                .GreaterThan(0)
                .WithMessage("Please select a valid branch.");
        }
    }
}
