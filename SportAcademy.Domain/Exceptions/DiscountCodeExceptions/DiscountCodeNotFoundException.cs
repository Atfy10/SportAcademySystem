using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Domain.Exceptions.DiscountCodeExceptions
{
    public class DiscountCodeNotFoundException : IdNotFoundException
    {
        static readonly string _entity = nameof(DiscountCode);

        public DiscountCodeNotFoundException(string id) : base(_entity, id) { }

        public DiscountCodeNotFoundException(string id, Exception innerException)
            : base(_entity, id, innerException) { }
    }
}
