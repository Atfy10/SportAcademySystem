using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class SessionOccurrence : IAuditableEntity
    {
        private List<Attendance> _attendances = [];

        private SessionOccurrence(
            int groupScheduleId,
            DateTime startDateTime,
            SessionStatus status)
        {
            GroupScheduleId = groupScheduleId;
            StartDateTime = startDateTime;
            Status = status;
            _attendances = [];
        }

        private SessionOccurrence() { }

        public int Id { get; private set; }
        public int GroupScheduleId { get; private set; }
        public DateTime StartDateTime { get; private set; }
        public SessionStatus Status { get; private set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public IReadOnlyCollection<Attendance> Attendances => _attendances.AsReadOnly();

        public virtual GroupSchedule GroupSchedule { get; set; } = null!;

        public static SessionOccurrence Create(
            int groupScheduleId,
            DateTime startDateTime,
            SessionStatus status)
        {
            return new SessionOccurrence(groupScheduleId, startDateTime, status);
        }

        public void Update(DateTime? startDateTime, SessionStatus? status)
        {
            if (startDateTime.HasValue)
                StartDateTime = startDateTime.Value;
            if (status.HasValue)
                Status = status.Value;
        }
    }
}
