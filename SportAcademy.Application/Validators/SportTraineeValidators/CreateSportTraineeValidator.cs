using FluentValidation;
using SportAcademy.Application.Commands.SportTraineeCommands.CreateSportTrainee;
using SportAcademy.Domain.Enums;
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

            // See UpdateSportTraineeValidator for why this is IsEnumName (string property, case-
            // insensitive) rather than IsInEnum() (which only validates actual enum types and
            // silently rejects every string value here).
            RuleFor(x => x.SkillLevel)
                .IsEnumName(typeof(SkillLevel), caseSensitive: false)
                .WithMessage("Invalid skill level. Please choose from the available options.");
        }
    }
}
