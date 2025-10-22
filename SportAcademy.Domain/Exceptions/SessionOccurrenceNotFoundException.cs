using SportAcademy.Domain.Entities;

namespace SportAcademy.Domain.Exceptions
{
    public class SessionOccurrenceNotFoundException : IdNotFoundException
    {
        static readonly string _entity = nameof(SessionOccurrence);

        public SessionOccurrenceNotFoundException(string id) : base(_entity, id) { }

        public SessionOccurrenceNotFoundException(string id, Exception innerException)
            : base(_entity, id, innerException) { }

    }
}
