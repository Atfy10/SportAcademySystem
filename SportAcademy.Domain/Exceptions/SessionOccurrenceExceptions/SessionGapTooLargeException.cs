namespace SportAcademy.Domain.Exceptions.SessionOccurrenceExceptions;

public class SessionGapTooLargeException : Exception
{
    private static string _message = "Last session for this group was on {LastOccurrenceDate}. Please specify a start date to generate from.";
    public DateOnly LastOccurrenceDate { get; }

    public SessionGapTooLargeException(DateOnly lastOccurrenceDate)
        : base(FormatMessage(lastOccurrenceDate))
    {
        LastOccurrenceDate = lastOccurrenceDate;
    }

    public SessionGapTooLargeException(DateOnly lastOccurrenceDate, Exception inner)
        : base(FormatMessage(lastOccurrenceDate), inner)
    {
        LastOccurrenceDate = lastOccurrenceDate;
    }

    private static string FormatMessage(DateOnly lastOccurrenceDate)
        => _message.Replace("{LastOccurrenceDate}", $"{lastOccurrenceDate:yyyy-MM-dd}");
}
