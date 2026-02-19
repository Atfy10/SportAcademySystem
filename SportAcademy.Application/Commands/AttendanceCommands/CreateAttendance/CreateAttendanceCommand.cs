using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.AttendanceCommands.CreateAttendance
{
    public record CreateAttendanceCommand(
        DateOnly? AttendanceDate,
        AttendanceStatus AttendanceStatus,
        TimeOnly? CheckInTime,
        string? CoachNote,
        int EnrollmentId,
        int SessionOccurrenceId
        ) : IRequest<Result<int>>;
}
