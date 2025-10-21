using FluentValidation;
using SportAcademy.Application.Commands.AttendanceCommands.CreateAttendance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.AttendanceValidators
{
    public class CreateAttendanceValidator : AbstractValidator<CreateAttendanceCommand>
    {
        public CreateAttendanceValidator()
        {
            RuleFor(a => a.AttendanceDate)
                .Cascade(CascadeMode.Stop)
                .ExclusiveBetween(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(-7)),
                    DateOnly.FromDateTime(DateTime.Now.AddDays(1))
                )
                .WithMessage("Attendance date cannot be in the future.");

            RuleFor(a => a.CheckInTime)
                .Cascade(CascadeMode.Stop)
                .ExclusiveBetween(
                    new TimeOnly(6, 0),
                    new TimeOnly(22, 0)
                )
                .WithMessage("Check-in time must be between 06:00 and 22:00.");

            RuleFor(a => a.CoachNote)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(500).WithMessage("Coach note must not exceed 500 characters.");

            RuleFor(a => a.AttendanceStatus)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Please select an attendance status.")
                .IsInEnum().WithMessage("Invalid option selected for attendance status.");

            RuleFor(a => a.EnrollmentId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Enrollment is required.")
                .GreaterThan(0).WithMessage("Enrollment ID must be a valid number.");

            RuleFor(a => a.SessionOccurrenceId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Session Occurrence is required.")
                .GreaterThan(0).WithMessage("Session Occurrence ID must be a valid number.");
        }
    }
}
