using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
    public class EmailExistException : Exception
    {
        static readonly string _message = "This email is already in use. Please try another one.";

        public EmailExistException() : base(_message)
        {

        }
        public EmailExistException(Exception innerException) : base(_message, innerException)
        {

        }

    }
}
