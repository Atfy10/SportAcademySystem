namespace SportAcademy.Domain.Exceptions.TraineeGroupExceptions
{
    public class GroupAtCapacityException : Exception
    {
        public GroupAtCapacityException(int traineeGroupId, int maximumCapacity)
            : base($"Trainee group {traineeGroupId} is at capacity ({maximumCapacity}). Remove a trainee or choose a different group before enrolling another.")
        {
        }
    }
}
