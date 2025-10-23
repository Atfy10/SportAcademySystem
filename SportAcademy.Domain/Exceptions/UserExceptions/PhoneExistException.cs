using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions.UserExceptions
{
    public class PhoneExistException : Exception
    {
        static readonly string _message = "This phone number is already linked to another account. Please use a different number.";

        public PhoneExistException() : base(_message)
        {

        }
        public PhoneExistException(Exception innerException) : base(_message,  innerException)
        {

        }

    }
}
