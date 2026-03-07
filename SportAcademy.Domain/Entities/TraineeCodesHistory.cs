namespace SportAcademy.Domain.Entities
{
    public class TraineeCodesHistory
    {
        public int Id { get; set; }
        public int TraineeId { get; set; }
        public string OldTraineeCode { get; set; } = null!;
        public DateTime ChangedAt { get; set; }
        public string? Reason { get; set; }

        // Navigation property
        public Trainee Trainee { get; set; } = null!;
    }
}
