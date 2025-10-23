using FluentValidation;
using SportAcademy.Application.Commands.EnrollmentCommands.DeleteEnrollment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.EnrollmentValidators
{
    public class DeleteEnrollmentValidator : AbstractValidator<DeleteEnrollmentCommand>
    {
        public DeleteEnrollmentValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Please provide an enrollment ID.")
                .GreaterThan(0).WithMessage("Please provide a valid enrollment ID.");
        }
    }
}
