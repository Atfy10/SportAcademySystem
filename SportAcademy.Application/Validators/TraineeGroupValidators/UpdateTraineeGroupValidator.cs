using FluentValidation;
using SportAcademy.Application.Commands.TraineeGroupCommands.UpdateTraineeGroup;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.TraineeGroupValidators
{
    public class UpdateTraineeGroupValidator : AbstractValidator<UpdateTraineeGroupCommand>
    {
        public UpdateTraineeGroupValidator(ITraineeGroupRepository traineeGroupRepository)
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

            // The ceiling depends on the group's own type, which the command no longer carries
            // (Type is fixed at creation), so it has to be read from the stored group rather
            // than taken from the request - otherwise a private group could be edited up to the
            // public limit and quietly exceed what private training is sold as.
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    if (!cmd.MaximumCapacity.HasValue) return true;

                    var group = await traineeGroupRepository.GetByIdAsync(cmd.Id, ct);
                    if (group is null) return true; // the handler reports the missing group itself

                    return group.Type != TraineeGroupType.Private
                        || cmd.MaximumCapacity.Value <= TraineeGroupCapacity.PrivateMaximum;
                })
                .WithMessage($"A private group cannot exceed {TraineeGroupCapacity.PrivateMaximum} trainees.");

            RuleFor(x => x.DurationInMinutes)
                .NotEmpty().WithMessage("Please enter the session duration.")
                .GreaterThan(30).WithMessage("Duration must be greater than 30.")
                .LessThanOrEqualTo(90).WithMessage("Duration cannot exceed 90 minutes.");

            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Please select a gender.")
                .IsInEnum().WithMessage("Invalid gender selected. Please choose from the available options.");

            RuleFor(x => x.CoachId)
                .NotEmpty().WithMessage("Please select a coach.")
                .GreaterThan(0).WithMessage("Coach ID must be a valid number.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Group name cannot be empty.")
                .MaximumLength(200).WithMessage("Group name can't exceed 200 characters.")
                .When(x => x.Name is not null);

            RuleFor(x => x.NameAr)
                .MaximumLength(150).WithMessage("Arabic group name can't exceed 150 characters.")
                .When(x => !string.IsNullOrEmpty(x.NameAr));
        }
    }
}
