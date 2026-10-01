using SportAcademy.Domain.Entities.Events;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Domain.Exceptions.EventExceptions
{
    public class EventNotFoundException : IdNotFoundException
    {
        static readonly string _entity = nameof(Event);

        public EventNotFoundException(string id) : base(_entity, id) { }

        public EventNotFoundException(string id, Exception innerException)
            : base(_entity, id, innerException) { }
    }
}
