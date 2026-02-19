using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AttendanceDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.AttendanceCommands.UpdateAttendance
{
    public record UpdateAttendanceCommand(
        int Id,
        string? CoachNote,
        int? SessionOccurrenceId,
        int? EnrollmentId
        ) : IRequest<Result<AttendanceDto>>;
}
