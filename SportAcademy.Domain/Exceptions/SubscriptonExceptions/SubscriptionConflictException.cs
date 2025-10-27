using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions.SubscriptonExceptions
{
    public class SubscriptionConflictException : Exception
    {
        private static readonly string _message = "The trainee has an active subscription for same sport that conflicts with the new subscription.";
        public SubscriptionConflictException() : base(_message) { }
        public SubscriptionConflictException(Exception innerException)
            : base(_message, innerException) { }
    }
}
