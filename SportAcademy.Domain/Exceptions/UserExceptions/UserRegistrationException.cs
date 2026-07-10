using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions.UserExceptions
{
    public class UserRegistrationException : Exception
    {
        public UserRegistrationException(List<string> errors) : base(string.Join("; ", errors))
        {

        }
        public UserRegistrationException(string message, Exception innerException) : base(message, innerException)
        {

        }

    }
}
