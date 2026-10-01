using SportAcademy.Domain.Entities.Events;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Domain.Exceptions.EventExceptions
{
    public class EventTicketNotFoundException : IdNotFoundException
    {
        static readonly string _entity = nameof(EventTicket);

        public EventTicketNotFoundException(string id) : base(_entity, id) { }

        public EventTicketNotFoundException(string id, Exception innerException)
            : base(_entity, id, innerException) { }
    }
}
