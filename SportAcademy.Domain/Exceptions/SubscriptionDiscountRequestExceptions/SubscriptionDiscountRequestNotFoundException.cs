using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Domain.Exceptions.SubscriptionDiscountRequestExceptions
{
    public class SubscriptionDiscountRequestNotFoundException : IdNotFoundException
    {
        static readonly string _entity = nameof(SubscriptionDiscountRequest);

        public SubscriptionDiscountRequestNotFoundException(string id) : base(_entity, id) { }

        public SubscriptionDiscountRequestNotFoundException(string id, Exception innerException)
            : base(_entity, id, innerException) { }
    }
}
