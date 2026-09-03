namespace SportAcademy.Domain.Exceptions.TraineeGroupExceptions
{
    public class TraineeGroupInactiveException : Exception
    {
        public TraineeGroupInactiveException(int traineeGroupId, string? reason)
            : base(reason is null
                ? $"Trainee group {traineeGroupId} is paused and isn't accepting new enrollments."
                : $"Trainee group {traineeGroupId} is paused and isn't accepting new enrollments: {reason}")
        {
        }
    }
}
