using SportAcademy.Domain.Contract;

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
    }
}
