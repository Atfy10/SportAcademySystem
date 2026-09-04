using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Domain.Exceptions.SalaryPaymentExceptions
{
    public class SalaryPaymentNotFoundException : IdNotFoundException
    {
        static readonly string _entity = nameof(SalaryPayment);

        public SalaryPaymentNotFoundException(string id) : base(_entity, id) { }

        public SalaryPaymentNotFoundException(string id, Exception innerException)
            : base(_entity, id, innerException) { }
    }
}
