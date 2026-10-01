using SportAcademy.Domain.Entities.Events;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Domain.Exceptions.EventExceptions
{
    public class EventCustomerNotFoundException : IdNotFoundException
    {
        static readonly string _entity = nameof(EventCustomer);

        public EventCustomerNotFoundException(string id) : base(_entity, id) { }

        public EventCustomerNotFoundException(string id, Exception innerException)
            : base(_entity, id, innerException) { }
    }
}
