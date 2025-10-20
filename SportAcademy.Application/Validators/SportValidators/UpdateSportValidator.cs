using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using SportAcademy.Application.Commands.SportCommands.UpdateSport;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Validators.SportValidators
{
	public class UpdateSportValidator : AbstractValidator<UpdateSportCommand>
	{
		public UpdateSportValidator()
		{
			RuleFor(x => x.Id)
				.GreaterThan(0).WithMessage("Sport Id must be greater than 0.");

			When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
			{
				RuleFor(x => x.Name)
					.MaximumLength(50).WithMessage("Sport name must not exceed 50 characters.");
			});

			When(x => !string.IsNullOrWhiteSpace(x.Description), () =>
			{
				RuleFor(x => x.Description)
					.MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
			});

			When(x => !string.IsNullOrWhiteSpace(x.Category), () =>
			{
				RuleFor(x => x.Category)
					.Must(category => Enum.IsDefined(typeof(SportCategory), category))
					.WithMessage("Invalid category specified.");
			});
		}
	}
}
