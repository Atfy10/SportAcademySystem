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
        public int SportSubscriptionTypeSubscriptionTypeId { get; set; }
        public int SportSubscriptionTypeSportId { get; set; }

        // Navigation Property
        public virtual Payment Payment { get; set; } = null!;
        public virtual Trainee Trainee { get; set; } = null!;
        public virtual SportSubscriptionType SportSubscriptionType { get; set; } = null!;
        public virtual Enrollment Enrollment { get; set; } = null!;
    }
}
