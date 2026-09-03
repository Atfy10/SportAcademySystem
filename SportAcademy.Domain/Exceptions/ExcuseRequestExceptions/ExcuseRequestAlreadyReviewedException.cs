namespace SportAcademy.Domain.Exceptions.ExcuseRequestExceptions
{
    public class ExcuseRequestAlreadyReviewedException : Exception
    {
        public ExcuseRequestAlreadyReviewedException(int id, string status)
            : base($"Excuse request {id} was already {status.ToLower()} and can't be reviewed again.")
        {
        }
    }
}
