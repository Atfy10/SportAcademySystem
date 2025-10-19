using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
    public class PaymentNotFoundException :Exception
    {
        static readonly string _message = "We couldn’t find an Payment with that Number. Please check and try again.";

        public PaymentNotFoundException() : base(_message)
        {

        }
        public PaymentNotFoundException(Exception innerException) : base(_message, innerException)
        {

        }
    }
}
