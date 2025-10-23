using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Domain.Exceptions.SubscriptonExceptions
{
    public class SubscriptionDetailsNotFoundException : IdNotFoundException
    {
        static readonly string _entity = nameof(SubscriptionDetails);

        public SubscriptionDetailsNotFoundException(string id) : base(_entity, id) { }

        public SubscriptionDetailsNotFoundException(string id, Exception innerException)
            : base(_entity, id, innerException) { }

    }
}
