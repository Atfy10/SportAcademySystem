using FluentValidation;
using SportAcademy.Application.Commands.AttendanceCommands.CreateAttendance;

namespace SportAcademy.Application.Validators.AttendanceValidators
{
    public class CreateAttendanceValidator : AbstractValidator<CreateAttendanceCommand>
    {
        public CreateAttendanceValidator()
        {
            RuleFor(a => a.SessionOccurrenceId)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Session Occurrence ID must be a valid number.");

            RuleFor(a => a.TraineeId)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Trainee ID must be a valid number.");

            RuleFor(a => a.Status)
                .IsInEnum().WithMessage("Invalid option selected for attendance status.");

            RuleFor(a => a.CheckInTime)
                .Must(value => value == null || TimeOnly.TryParse(value, out _))
                .WithMessage("Check-in time must be a valid time (HH:mm).")
                .Must(value => value == null || !TimeOnly.TryParse(value, out var t) ||
                    (t >= new TimeOnly(6, 0) && t <= new TimeOnly(22, 0)))
                .WithMessage("Check-in time must be between 06:00 and 22:00.");
        }
    }
}
