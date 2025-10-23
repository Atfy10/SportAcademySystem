using FluentValidation;
using SportAcademy.Application.Commands.EmployeeCommands.DeleteEmployee;
using SportAcademy.Application.Commands.SportCommands.DeleteSport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.SportValidators
{
    public class DeleteSportValidator : AbstractValidator<DeleteSportCommand>
    {
        public DeleteSportValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Please provide a sport ID.")
                .GreaterThan(0).WithMessage("Please provide a valid sport ID.");
        }
    }
}
