using FluentValidation;
using SportAcademy.Application.Commands.CoachCommands.CreateCoach;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.CoachValidators
{
    public class CreateCoachValidator : AbstractValidator<CreateCoachCommand>
    {
        public CreateCoachValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(c => c.SkillLevel)
                .NotEmpty().WithMessage("Skill level is required.")
                .IsInEnum()
                .WithMessage("Invalid skill level.");

            RuleFor(c => c.SportId)
                .ApplyIdRuleFor("Sport");

            RuleFor(c => c.EmployeeId)
                .ApplyIdRuleFor("Employee");
        }
    }
}
