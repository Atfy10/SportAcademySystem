using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    /// <summary>
    /// A coach-filed request to excuse a trainee's absence from a session. Marking attendance as
    /// Excused never writes an Attendance row directly - it creates one of these instead, pending
    /// Owner/Admin approval. Approving it writes the Attendance row and defers the trainee's
    /// subscription by one session; rejecting it discards the request so the coach can mark the
    /// trainee's real status (e.g. Absent) instead.
    /// </summary>
    public class ExcuseRequest : ITenantScoped, IAuditableEntity
    {
        public int Id { get; set; }
        public int SessionOccurrenceId { get; set; }
        public int TraineeId { get; set; }
        public int EnrollmentId { get; set; }
        public string Reason { get; set; } = null!;
        public ExcuseRequestStatus Status { get; set; } = ExcuseRequestStatus.Pending;
        public string? RequestedByUserId { get; set; }
        public string? ReviewedByUserId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNote { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        // Navigation Properties
        public virtual SessionOccurrence SessionOccurrence { get; set; } = null!;
        public virtual Trainee Trainee { get; set; } = null!;
        public virtual Enrollment Enrollment { get; set; } = null!;
    }
}
