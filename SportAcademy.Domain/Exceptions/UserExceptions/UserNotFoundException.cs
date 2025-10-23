using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions.UserExceptions
{
    public class UserNotFoundException : Exception
    {
        static readonly string _message = "We couldn’t find a matching user. Please check your details and try again.";

        public UserNotFoundException() : base(_message)
        {
        }
        public UserNotFoundException(Exception innerException) : base(_message, innerException)
        {
        }
    }
}
