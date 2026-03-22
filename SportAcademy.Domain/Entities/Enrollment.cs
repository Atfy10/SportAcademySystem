using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class Enrollment : IAuditableEntity, ISoftDeletable
    {
        public int Id { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int SessionAllowed { get; set; }
        public int SessionRemaining { get; set; }
        public bool IsActive { get; set; }
        public int TraineeId { get; set; }
        public int TraineeGroupId { get; set; }
        public int SubscriptionDetailsId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        // Navigation Properties
        public virtual Trainee Trainee { get; set; } = null!;
        public virtual TraineeGroup TraineeGroup { get; set; } = null!;
        public virtual ICollection<Attendance> Attendances { get; set; } = [];
        public virtual SubscriptionDetails SubscriptionDetails { get; set; } = null!;

        public string GetPaymentStatus()
        {
            if (ExpiryDate < DateTime.UtcNow) return "Overdue";
            if (SubscriptionDetails != null && SubscriptionDetails.Payment != null) return "Paid";
            return "Pending";
        }

        public string GetStatus()
        {
            if (ExpiryDate < DateTime.UtcNow) return "Expired";
            if (!IsActive) return "Suspended";
            return "Active";
        }

        public int GetSessionsCompleted()
            => Attendances.Count(a =>
                a.AttendanceStatus == AttendanceStatus.Present ||
                a.AttendanceStatus == AttendanceStatus.Late);

    }
}
