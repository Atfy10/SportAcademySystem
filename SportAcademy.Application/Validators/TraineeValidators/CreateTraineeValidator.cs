using FluentValidation;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.TraineeValidators
{
    public class CreateTraineeValidator : AbstractValidator<CreateTraineeCommand>
    {
        public CreateTraineeValidator()
        {
            RuleFor(t => t.FirstName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(t => t.LastName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(t => t.SSN)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("SSN is required.")
                .Length(12).WithMessage("SSN length must be 12.");

            RuleFor(t => t.BirthDate)
                .Cascade(CascadeMode.Stop)
                .LessThan(DateOnly.FromDateTime(DateTime.Now))
                .WithMessage("Birth date must be in the past.");

            RuleFor(t => t.GuardianName)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(50).WithMessage("Guardian name cannot exceed 50 characters.")
                .When(t => !string.IsNullOrEmpty(t.GuardianName));

            RuleFor(t => t.ParentNumber)
                .Cascade(CascadeMode.Stop)
                .Length(8).WithMessage("Phone number cannot exceed 8 characters.")
                .When(t => !string.IsNullOrEmpty(t.ParentNumber));
        }
    }
}
