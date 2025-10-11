using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class SessionOccurrence
    {
        public int Id { get; set; }
        public int GroupScheduleId { get; set; }
        public DateTime StartDateTime { get; set; }
        public SessionStatus Status { get; set; } = SessionStatus.Scheduled;

        // Navigation Property
        public virtual GroupSchedule GroupSchedule { get; set; } = null!;
        public virtual ICollection<Attendance> Attendances { get; set; } = [];
    }
}
