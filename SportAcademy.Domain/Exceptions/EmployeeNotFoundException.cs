using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
    public class EmployeeNotFoundException : Exception
    {
        static readonly string _message = "We couldn’t find a matching employee. Please check your details and try again.";

        public EmployeeNotFoundException() : base(_message)
        {
        }
        public EmployeeNotFoundException(Exception innerException) : base(_message, innerException)
        {
        }

    }
}
