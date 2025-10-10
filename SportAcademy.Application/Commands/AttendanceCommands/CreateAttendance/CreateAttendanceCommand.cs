using MediatR;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.AttendanceCommands.CreateAttendance
{
    public record CreateAttendanceCommand(
        AttendanceStatus Status, 
        TimeOnly? CheckInTime,
        string CoachNote,
        int? EnrollmentId
        ) : IRequest<Result<int>>;
}
