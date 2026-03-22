namespace SportAcademy.Domain.Exceptions.SessionOccurrenceExceptions;

public class NoSchedulesFoundException : Exception
{
    public NoSchedulesFoundException()
        : base("No group schedules found for the specified group.") { }

    public NoSchedulesFoundException(Exception inner)
        : base("No group schedules found for the specified group.", inner) { }
}
