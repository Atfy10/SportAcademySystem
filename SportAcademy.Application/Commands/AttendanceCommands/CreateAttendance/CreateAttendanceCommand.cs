using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.AttendanceCommands.CreateAttendance
{
    // Trainee-based, mirroring BulkCreateAttendanceCommand's AttendanceItem shape: the caller
    // (a coach marking a session roster) only ever knows the trainee and session occurrence,
    // never the underlying EnrollmentId, so the handler resolves that internally.
    public record CreateAttendanceCommand(
        int SessionOccurrenceId,
        int TraineeId,
        AttendanceStatus Status,
        string? CheckInTime
        ) : IRequest<Result<int>>;
}
