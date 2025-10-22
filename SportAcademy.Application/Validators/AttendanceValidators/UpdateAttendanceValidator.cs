using FluentValidation;
using SportAcademy.Application.Commands.AttendanceCommands.UpdateAttendance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.AttendanceValidators
{
    public class UpdateAttendanceValidator : AbstractValidator<UpdateAttendanceCommand>
    {
        public UpdateAttendanceValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(a => a.Id)
                .GreaterThan(0)
                .WithMessage("Invalid attendance ID.");

            RuleFor(a => a.SessionOccurrenceId)
                .GreaterThan(0)
                .WithMessage("Please select a valid session occurrence.");

            RuleFor(a => a.EnrollmentId)
                .GreaterThan(0)
                .WithMessage("Please select a valid enrollment.");

            RuleFor(a => a.CoachNote)
                .MaximumLength(500)
                .WithMessage("Coach note cannot exceed 500 characters.")
                .When(a => !string.IsNullOrWhiteSpace(a.CoachNote));
        }
    }
}
