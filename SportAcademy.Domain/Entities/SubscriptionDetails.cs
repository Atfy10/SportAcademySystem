namespace SportAcademy.Domain.Entities
{
    public class SubscriptionDetails
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

        // Navigation Property
        public virtual Payment Payment { get; set; } = null!;
        public virtual Trainee Trainee { get; set; } = null!;
        public virtual SportPrice SportPrice { get; set; } = null!;
        public virtual Enrollment Enrollment { get; set; } = null!;
    }
}
