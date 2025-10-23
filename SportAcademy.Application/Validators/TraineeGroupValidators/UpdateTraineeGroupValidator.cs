using FluentValidation;
using SportAcademy.Application.Commands.TraineeGroupCommands.UpdateTraineeGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.TraineeGroupValidators
{
    public class UpdateTraineeGroupValidator : AbstractValidator<UpdateTraineeGroupCommand>
    {
        public UpdateTraineeGroupValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Please provide the trainee group ID.")
                .GreaterThan(0).WithMessage("Trainee group ID must be a valid positive number.");

            RuleFor(x => x.SkillLevel)
                .NotEmpty().WithMessage("Please select a skill level.")
                .IsInEnum().WithMessage("Invalid skill level selected. Please choose from the available options.");

            RuleFor(x => x.MaximumCapacity)
                .NotEmpty().WithMessage("Please enter the maximum capacity.")
                .GreaterThan(0).WithMessage("Maximum capacity must be greater than 0.")
                .LessThanOrEqualTo(15).WithMessage("Maximum capacity cannot exceed 15 trainees.");

            RuleFor(x => x.DurationInMinutes)
                .NotEmpty().WithMessage("Please enter the session duration.")
                .GreaterThan(30).WithMessage("Duration must be greater than 30.")
                .LessThanOrEqualTo(90).WithMessage("Duration cannot exceed 90 minutes.");

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
