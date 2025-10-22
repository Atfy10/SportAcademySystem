using FluentValidation;
using SportAcademy.Application.Commands.SportTraineeCommands.CreateSportTrainee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.SportTraineeValidators
{
    public class CreateSportTraineeValidator : AbstractValidator<CreateSportTraineeCommand>
    {
        public CreateSportTraineeValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.SportId)
                .GreaterThan(0)
                .WithMessage("Please select a valid sport.");

            RuleFor(x => x.TraineeId)
                .GreaterThan(0)
                .WithMessage("Please select a valid trainee.");

            RuleFor(x => x.SkillLevel)
                .IsInEnum()
                .WithMessage("Invalid skill level. Please choose from the available options.");
        }
    }
}
