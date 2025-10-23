using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions.SharedExceptions
{
    public class SSNNotUniqueException : Exception
    {
        static readonly string _message = "SSN is exist. Please provide a correct SSN.";
        public SSNNotUniqueException() : base(_message)
        {
        }
        public SSNNotUniqueException(Exception innerException) : base(_message, innerException)
        {
        }
    }
}
