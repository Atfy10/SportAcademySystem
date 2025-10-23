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
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Please provide a sport name.")
                .MaximumLength(100).WithMessage("Sport name can't exceed 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description can't exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Please select a sport category.")
                .IsInEnum().WithMessage("Please select a valid sport category.");
        }
    }
}
