using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions.GeneralExceptions
{
    public class ValidationException : Exception
    {
        static readonly string _message = "Some information you entered is invalid. Please review and try again.";

        public ValidationException() : base(_message)
        {

        }
        public ValidationException(Exception innerException) : base(_message, innerException)
        {

        }
    }
}
