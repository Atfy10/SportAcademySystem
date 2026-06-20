namespace SportAcademy.Domain.Entities
{
    public class TraineeCodesHistory
    {
        public int Id { get; private set; }
        public int TraineeId { get; private set; }
        public string OldTraineeCode { get; private set; } = null!;
        public DateTime ChangedAt { get; private set; }
        public string? Reason { get; private set; }

        public Trainee Trainee { get; set; } = null!;

        private TraineeCodesHistory() { }

        private TraineeCodesHistory(int traineeId, string oldTraineeCode, string? reason)
        {
            TraineeId = traineeId;
            OldTraineeCode = oldTraineeCode;
            ChangedAt = DateTime.UtcNow;
            Reason = reason;
        }

        public static TraineeCodesHistory Create(int traineeId, string oldTraineeCode, string? reason = null)
            => new(traineeId, oldTraineeCode, reason);
    }
}
