using FluentValidation;
using SportAcademy.Application.Commands.SportTraineeCommands.CreateSportTrainee;
using SportAcademy.Application.Commands.SportTraineeCommands.UpdateSportTrainee;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.SportTraineeValidators
{
    public class UpdateSportTraineeValidator : AbstractValidator<UpdateSportTraineeCommand>
    {
        public UpdateSportTraineeValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.SportId)
                .GreaterThan(0)
                .WithMessage("Please select a valid sport.");

            RuleFor(x => x.TraineeId)
                .GreaterThan(0)
                .WithMessage("Please select a valid trainee.");

            // SkillLevel is a string, not the enum itself (see CreateSportTraineeCommandHandler
            // for why - the handler needs a case-insensitive parse since wire values are
            // camelCase). IsInEnum() only validates actual enum-typed properties and silently
            // rejects every string value here, so match the handler's case-insensitive check.
            RuleFor(x => x.SkillLevel)
                .IsEnumName(typeof(SkillLevel), caseSensitive: false)
                .WithMessage("Invalid skill level. Please choose from the available options.");
        }
    }
}
