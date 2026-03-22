namespace SportAcademy.Domain.Exceptions.SessionOccurrenceExceptions;

public class InvalidDurationException : Exception
{
    public int MinDays { get; }
    public int MaxDays { get; }

    public InvalidDurationException(int minDays, int maxDays)
        : base($"Duration must be between {minDays} and {maxDays} days.")
    {
        MinDays = minDays;
        MaxDays = maxDays;
    }

    public InvalidDurationException(int minDays, int maxDays, Exception inner)
        : base($"Duration must be between {minDays} and {maxDays} days.", inner)
    {
        MinDays = minDays;
        MaxDays = maxDays;
    }
}
