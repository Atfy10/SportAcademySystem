    using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class Attendance
    {
        public int Id { get; set; }
        public DateTime AttendanceDate { get; set; }
        public AttendanceStatus AttendanceStatus { get; set; }
        public TimeOnly CheckInTime { get; set; }
        public required string CoachNote { get; set; }
        public int EnrollmentId { get; set; }
        public int SessionOccurrenceId { get; set; }

        // Navigation Properties
        public virtual Enrollment Enrollment { get; set; } = null!;
        public virtual SessionOccurrence SessionOccurrence { get; set; } = null!;
    }
}
