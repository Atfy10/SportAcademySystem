using FluentValidation;
using SportAcademy.Application.Commands.EnrollmentCommands.UpdateEnrollment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.EnrollmentValidators
{
    public class UpdateEnrollmentValidator : AbstractValidator<UpdateEnrollmentCommand>
    {
        public UpdateEnrollmentValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Please provide an enrollment ID.")
                .GreaterThan(0).WithMessage("Please provide a valid enrollment ID.");

            RuleFor(x => x.ExpiryDate)
                .NotEmpty().WithMessage("Please provide an expiry date.")
                .Must(x => x > DateTime.Now)
                .WithMessage("Expiry date should be in the future.");

            RuleFor(x => x.SessionRemaining)
                .NotEmpty().WithMessage("Please specify the remaining sessions.")
                .GreaterThanOrEqualTo(0).WithMessage("Remaining sessions can't be negative.");
        }
    }
}
