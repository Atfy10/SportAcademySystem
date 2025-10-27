using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions.PaymentExceptions
{
    public class PaymentConflictException : Exception
    {
        private static string CreateMessage(string? paymentId = null)
                => paymentId is null
                    ? "This payment is already associated with another subscription."
                    : $"Payment with ID '{paymentId}' is already associated with another subscription.";

        public PaymentConflictException()
            : base(CreateMessage()) { }

        public PaymentConflictException(string paymentId)
            : base(CreateMessage(paymentId)) { }

        public PaymentConflictException(string paymentId, Exception innerException)
            : base(CreateMessage(paymentId), innerException) { }
    }
}
