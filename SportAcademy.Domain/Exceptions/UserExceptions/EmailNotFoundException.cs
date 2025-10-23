using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions.UserExceptions
{
    public class EmailNotFoundException : Exception
    {
        static readonly string _message = "We couldn’t find an account with that email. Please check and try again.";

        public EmailNotFoundException() : base(_message)
        {

        }
        public EmailNotFoundException(Exception innerException) : base(_message, innerException)
        {

        }

    }
}
