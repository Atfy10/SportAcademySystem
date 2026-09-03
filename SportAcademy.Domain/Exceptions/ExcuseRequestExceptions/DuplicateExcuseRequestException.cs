namespace SportAcademy.Domain.Exceptions.ExcuseRequestExceptions
{
    public class DuplicateExcuseRequestException : Exception
    {
        public DuplicateExcuseRequestException(int sessionOccurrenceId, int traineeId)
            : base($"Trainee {traineeId} already has a pending excuse request for session {sessionOccurrenceId}.")
        {
        }
    }
}
