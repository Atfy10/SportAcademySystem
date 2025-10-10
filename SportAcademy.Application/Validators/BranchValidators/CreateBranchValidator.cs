using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using SportAcademy.Application.Commands.BranchCommands.CreateBranch;

namespace SportAcademy.Application.Validators.BranchValidators
{
	public class CreateBranchValidator : AbstractValidator<CreateBranchCommand>
	{
		public CreateBranchValidator()
		{
			RuleFor(x => x.Name)
				.NotEmpty().WithMessage("Branch name is required.")
				.MaximumLength(100);

			RuleFor(x => x.City)
				.NotEmpty().WithMessage("City is required.")
				.MaximumLength(50);

			RuleFor(x => x.Country)
				.NotEmpty().WithMessage("Country is required.")
				.MaximumLength(50);

			RuleFor(x => x.PhoneNumber)
				.NotEmpty().WithMessage("Phone number is required.")
				.Matches(@"^\+?[0-9]{8,15}$").WithMessage("Invalid phone number format.");

			RuleFor(x => x.Email)
				.EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
				.WithMessage("Invalid email format.");

			RuleFor(x => x.CoX)
				.NotEmpty().WithMessage("CoX is required.");

			RuleFor(x => x.CoY)
				.NotEmpty().WithMessage("CoY is required.");
		}

		}
}
