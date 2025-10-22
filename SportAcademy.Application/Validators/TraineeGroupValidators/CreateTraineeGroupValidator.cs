using FluentValidation;
using SportAcademy.Application.Commands.TraineeGroupCommands.CreateTraineeGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.TraineeGroupValidators
{
    public class CreateTraineeGroupValidator : AbstractValidator<CreateTraineeGroupCommand>
    {
        public CreateTraineeGroupValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.SkillLevel)
                .NotEmpty().WithMessage("Please select a skill level.")
                .IsInEnum().WithMessage("Invalid skill level selected. Please choose from the available options.");

            RuleFor(x => x.MaximumCapacity)
                .NotEmpty().WithMessage("Please enter the maximum capacity.")
                .GreaterThan(0).WithMessage("Maximum capacity must be greater than 0.")
                .LessThanOrEqualTo(50).WithMessage("Maximum capacity cannot exceed 50 trainees.");

            RuleFor(x => x.DurationInMinutes)
                .NotEmpty().WithMessage("Please enter the session duration.")
                .GreaterThan(0).WithMessage("Duration must be greater than 0.")
                .LessThanOrEqualTo(180).WithMessage("Duration cannot exceed 180 minutes.");

            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Please select a gender.")
                .IsInEnum().WithMessage("Invalid gender selected. Please choose from the available options.");

            RuleFor(x => x.BranchId)
                .NotEmpty().WithMessage("Please select a branch.")
                .GreaterThan(0).WithMessage("Branch ID must be a valid number.");

            RuleFor(x => x.CoachId)
                .NotEmpty().WithMessage("Please select a coach.")
                .GreaterThan(0).WithMessage("Coach ID must be a valid number.");
        }
    }
}
