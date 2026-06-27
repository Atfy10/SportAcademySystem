using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class SessionOccurrence : ITenantScoped, IAuditableEntity
    {
        public int Id { get; set; }
        public int GroupScheduleId { get; set; }
        public DateTime StartDateTime { get; set; }
        public SessionStatus Status { get; set; } = SessionStatus.Scheduled;
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        // Navigation Property
        public virtual GroupSchedule GroupSchedule { get; set; } = null!;
        public virtual ICollection<Attendance> Attendances { get; set; } = [];
    }
}
