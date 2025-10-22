using FluentValidation;
using SportAcademy.Application.Commands.SessionOccurrenceCommands.DeleteSessionOccurence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.SessionOccurrenceValidators
{
    public class DeleteSessionOccurrenceValidator : AbstractValidator<DeleteSessionOccurrenceCommand>
    {
        public DeleteSessionOccurrenceValidator()
        {
            RuleFor(x => x.Id)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Please provide the SessionOccurrence ID.")
                .GreaterThan(0).WithMessage("SessionOccurrence ID must be greater than zero.");
        }
    }
}
