using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using SportAcademy.Application.Commands.SportCommands.CreateSport;

namespace SportAcademy.Application.Validators.SportValidators
{
	public class CreateSportValidator : AbstractValidator<CreateSportCommand>
	{
		public CreateSportValidator()
		{
			RuleFor(x => x.Name)
				.NotEmpty().WithMessage("Sport name is required.")
				.MaximumLength(50).WithMessage("Sport name must not exceed 50 characters.");

			RuleFor(x => x.Description)
				.MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
				.When(x => !string.IsNullOrWhiteSpace(x.Description));

			RuleFor(x => x.Category)
				.NotEmpty().WithMessage("Category is required.")
				.Must(category => Enum.IsDefined(typeof(Domain.Enums.SportCategory), category))
				.WithMessage("Invalid category specified.");

			RuleFor(x => x.IsRequireHealthTest)
				.NotNull().WithMessage("IsRequireHealthTest must be specified.");
		}


	}
}
