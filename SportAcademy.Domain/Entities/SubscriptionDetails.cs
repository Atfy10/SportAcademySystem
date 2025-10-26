using SportAcademy.Domain.Contract;

namespace SportAcademy.Domain.Entities
{
    public class SubscriptionDetails : IAuditableEntity, ISoftDeletable
    {
        public int Id { get; set; }
        public required DateOnly StartDate { get; set; }
        public required DateOnly EndDate { get; set; }
        public bool IsActive { get; } = true;
        public required string PaymentNumber { get; set; }
        public int TraineeId { get; set; }
        public int SubscriptionTypeId { get; set; }
        public int SportId { get; set; }
        public int BranchId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation Property
        public virtual Payment Payment { get; set; } = null!;
        public virtual Trainee Trainee { get; set; } = null!;
        public virtual SportPrice SportPrice { get; set; } = null!;
        public virtual Enrollment Enrollment { get; set; } = null!;
    }
}
