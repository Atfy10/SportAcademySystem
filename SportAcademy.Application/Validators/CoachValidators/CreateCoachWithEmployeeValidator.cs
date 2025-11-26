using FluentValidation;
using SportAcademy.Application.Commands.CoachCommands.CreateCoachWithEmployee;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.Validators.EmployeeValidators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.CoachValidators
{
    public class CreateCoachWithEmployeeValidator : AbstractValidator<CreateCoachWithEmployeeCommand>
    {
        public CreateCoachWithEmployeeValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(c => c.SkillLevel)
                .NotEmpty().WithMessage("Skill level is required.")
                .IsInEnum()
                .WithMessage("Invalid skill level.");

            RuleFor(c => c.SportId)
                .ApplyIdRuleFor("Sport");

            RuleFor(c => c.Employee)
                .SetValidator(new CreateEmployeeDtoValidator());
        }
    }
}
