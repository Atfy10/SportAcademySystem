using SportAcademy.Application.Commands.AttendanceCommands.CreateAttendance;
using SportAcademy.Application.DTOs.AttendanceDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings
{
    public static class AttendanceMappings
    {
        public static Attendance ToAttendance(this CreateAttendanceCommand cmd)
        {
            return Attendance.Create(
                cmd.EnrollmentId,
                cmd.SessionOccurrenceId,
                cmd.AttendanceStatus,
                cmd.AttendanceDate?.ToDateTime(TimeOnly.MinValue),
                cmd.CheckInTime,
                cmd.CoachNote);
        }

        public static AttendanceDto ToDto(this Attendance attendance)
        {
            return new AttendanceDto(
                attendance.Id,
                attendance.AttendanceDate,
                attendance.AttendanceStatus.ToString(),
                attendance.CheckInTime.ToString("HH:mm:ss"),
                attendance.CoachNote ?? string.Empty,
                attendance.EnrollmentId,
                attendance.SessionOccurrenceId);
        }
    }
}
