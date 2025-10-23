using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Domain.Exceptions.SessionOccurrenceExceptions
{
    public class SessionOccurrenceNotFoundException : IdNotFoundException
    {
        static readonly string _entity = nameof(SessionOccurrence);

        public SessionOccurrenceNotFoundException(string id) : base(_entity, id) { }

        public SessionOccurrenceNotFoundException(string id, Exception innerException)
            : base(_entity, id, innerException) { }

    }
}
