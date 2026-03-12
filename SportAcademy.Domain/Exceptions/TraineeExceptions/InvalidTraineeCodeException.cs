namespace SportAcademy.Domain.Exceptions.TraineeExceptions
{
    public sealed class InvalidTraineeCodeException : Exception
    {
        public InvalidTraineeCodeException(string value)
            : base($"Invalid trainee code format: '{value}'.")
        {
        }
    }
}
