using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions.PaymentExceptions
{
    public class PaymentNotFoundException : IdNotFoundException
    {
        private static readonly string _entity = nameof(Payment);

        public PaymentNotFoundException(string id) : base(_entity, id) { }

        public PaymentNotFoundException(string id, Exception innerException)
            : base(_entity, id, innerException) { }
    }
}