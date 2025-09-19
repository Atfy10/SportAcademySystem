using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
    public class GuardianInfoMissingException : Exception
    {
        static readonly string _message = "Guardian information is required for trainees under 18 years old. Please provide the guardian's name and contact number.";
        public GuardianInfoMissingException() : base(_message)
        {
        }
        public GuardianInfoMissingException(Exception innerException) : base(_message, innerException)
        {
        }
    }
}
