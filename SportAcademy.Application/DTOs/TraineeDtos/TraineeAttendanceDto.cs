using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.TraineeDtos
{
    public class TraineeAttendanceDto
    {
        public DateTime StartDateTime { get; set; }

        public SessionStatus Status { get; set; }

        public DateOnly AttendanceDate { get; set; }

        public AttendanceStatus AttendanceStatus { get; set; }

        public TimeOnly? CheckInTime { get; set; }

        public string? CoachNote { get; set; }
    }
}
