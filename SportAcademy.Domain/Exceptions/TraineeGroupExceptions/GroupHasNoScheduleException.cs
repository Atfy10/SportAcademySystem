namespace SportAcademy.Domain.Exceptions.TraineeGroupExceptions
{
    public class GroupHasNoScheduleException : Exception
    {
        public GroupHasNoScheduleException()
            : base("No training days were provided. A subscription's end date is worked out by counting " +
                   "its sessions across the days the group actually trains, so at least one training day " +
                   "is required.")
        {
        }

        public GroupHasNoScheduleException(int traineeGroupId)
            : base($"Trainee group {traineeGroupId} has no weekly schedule set up yet. Add its training " +
                   "days before enrolling trainees - the enrollment period is counted across them.")
        {
        }
    }
}
