using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using SportAcademy.Application.Commands.BranchCommands.UpdateBranch;

namespace SportAcademy.Application.Validators.BranchValidators
{
	public class UpdateBranchValidator : AbstractValidator<UpdateBranchCommand>
	{
		public UpdateBranchValidator()
		{
			RuleFor(b => b.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Branch name is required.")
				.MaximumLength(100).WithMessage("Branch name cannot exceed 100 characters.");

			RuleFor(b => b.City)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("City is required.")
				.MaximumLength(50).WithMessage("City cannot exceed 50 characters.");

			RuleFor(b => b.Country)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Country is required.")
				.MaximumLength(50).WithMessage("Country cannot exceed 50 characters.");

			RuleFor(b => b.PhoneNumber)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Phone number is required.")
				.Length(8).WithMessage("Phone number must be exactly 8 characters.");

			RuleFor(b => b.Email)
                .Cascade(CascadeMode.Stop)
                .EmailAddress().WithMessage("Invalid email format.")
				.MaximumLength(100).WithMessage("Email cannot exceed 100 characters.")
				.When(b => !string.IsNullOrEmpty(b.Email));

			RuleFor(b => b.CoX)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Coordinate X is required.")
				.MaximumLength(20).WithMessage("Coordinate X cannot exceed 20 characters.");

			RuleFor(b => b.CoY)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Coordinate Y is required.")
				.MaximumLength(20).WithMessage("Coordinate Y cannot exceed 20 characters.");
		}
}
}
