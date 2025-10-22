using SportAcademy.Domain.Entities;

namespace SportAcademy.Domain.Exceptions
{
    public class SubscriptionDetailsNotFoundException : IdNotFoundException
    {
        static readonly string _entity = nameof(SubscriptionDetails);

        public SubscriptionDetailsNotFoundException(string id) : base(_entity, id) { }

        public SubscriptionDetailsNotFoundException(string id, Exception innerException)
            : base(_entity, id, innerException) { }

    }
}
