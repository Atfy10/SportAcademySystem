using FluentValidation;
using SportAcademy.Application.Commands.TraineeGroupCommands.DeleteTraineeGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.TraineeGroupValidators
{
    public class DeleteTraineeGroupValidator : AbstractValidator<DeleteTraineeGroupCommand>
    {
        public DeleteTraineeGroupValidator()
        {
            RuleFor(x => x.Id)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Please provide the trainee group ID.")
                .GreaterThan(0).WithMessage("Trainee group ID must be a valid positive number.");
        }
    }
}
