namespace SportAcademy.Domain.Exceptions.SharedExceptions;

public class InvalidSearchTermException : Exception
{
    public int MinLength { get; }

    public InvalidSearchTermException(int minLength)
        : base($"Search term must be at least {minLength} characters.")
    {
        MinLength = minLength;
    }

    public InvalidSearchTermException(int minLength, Exception inner)
        : base($"Search term must be at least {minLength} characters.", inner)
    {
        MinLength = minLength;
    }
}
