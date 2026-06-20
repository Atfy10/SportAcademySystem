using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class Attendance : IAuditableEntity
    {
        private Attendance(
            DateTime attendanceDate,
            AttendanceStatus attendanceStatus,
            TimeOnly checkInTime,
            string coachNote,
            int enrollmentId,
            int sessionOccurrenceId)
        {
            AttendanceDate = attendanceDate;
            AttendanceStatus = attendanceStatus;
            CheckInTime = checkInTime;
            CoachNote = coachNote;
            EnrollmentId = enrollmentId;
            SessionOccurrenceId = sessionOccurrenceId;
        }

        private Attendance() { }

        public int Id { get; private set; }
        public DateTime AttendanceDate { get; private set; }
        public AttendanceStatus AttendanceStatus { get; private set; }
        public TimeOnly CheckInTime { get; private set; }
        public string CoachNote { get; private set; } = string.Empty;
        public int EnrollmentId { get; private set; }
        public int SessionOccurrenceId { get; private set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual Enrollment Enrollment { get; set; } = null!;
        public virtual SessionOccurrence SessionOccurrence { get; set; } = null!;

        public static Attendance Create(
            int enrollmentId,
            int sessionOccurrenceId,
            AttendanceStatus status,
            DateTime? attendanceDate = null,
            TimeOnly? checkInTime = null,
            string? coachNote = null)
        {
            return new Attendance(
                attendanceDate ?? DateTime.UtcNow,
                status,
                checkInTime ?? TimeOnly.FromDateTime(DateTime.UtcNow),
                coachNote ?? string.Empty,
                enrollmentId,
                sessionOccurrenceId);
        }

        public void UpdateStatus(AttendanceStatus status, string? checkInTime = null)
        {
            AttendanceStatus = status;
            if (checkInTime != null)
                CheckInTime = TimeOnly.Parse(checkInTime);
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDetails(string? coachNote, int? enrollmentId, int? sessionOccurrenceId)
        {
            if (coachNote != null)
                CoachNote = coachNote;
            if (enrollmentId.HasValue)
                EnrollmentId = enrollmentId.Value;
            if (sessionOccurrenceId.HasValue)
                SessionOccurrenceId = sessionOccurrenceId.Value;
        }
    }
}
