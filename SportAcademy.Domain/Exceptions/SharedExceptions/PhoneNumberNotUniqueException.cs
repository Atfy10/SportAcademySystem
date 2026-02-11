using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions.SharedExceptions
{
    public class PhoneNumberNotUniqueException : Exception
    {
        private static readonly string _message = "Phone number is for other person, please enter another one.";
        public PhoneNumberNotUniqueException() : base(_message)
        {
        }
        public PhoneNumberNotUniqueException(Exception innerException) : base(_message, innerException)
        {
        }
    }
}
