using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
    public class SSNSyntaxErrorException : Exception
    {
        static readonly string _message = "SSN is invalid due syntax error.";

        public SSNSyntaxErrorException() : base(_message)
        {
        }
        public SSNSyntaxErrorException(Exception innerException) : base(_message, innerException)
        {
        }
    }
}
