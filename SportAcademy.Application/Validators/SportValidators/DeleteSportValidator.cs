using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using SportAcademy.Application.Commands.EmployeeCommands.DeleteEmployee;

namespace SportAcademy.Application.Validators.SportValidators
{
	public class DeleteSportValidator : AbstractValidator<DeleteEmployeeCommand>
	{
		public DeleteSportValidator()
		{
			RuleFor(x => x.Id)
				.GreaterThan(0).WithMessage("Sport Id must be greater than 0.");
		}
	}
}
