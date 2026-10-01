using FluentValidation;
using SportAcademy.Application.Commands.EmployeeCommands.ChangeEmployeePosition;

namespace SportAcademy.Application.Validators.EmployeeValidators
{
    public class ChangeEmployeePositionValidator : AbstractValidator<ChangeEmployeePositionCommand>
    {
        public ChangeEmployeePositionValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Invalid employee ID.");

            RuleFor(x => x.Position)
                .IsInEnum()
                .WithMessage("Invalid position value.");
        }
    }
}
