using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions.UserExceptions
{
    public class UserRegistrationException : Exception
    {
        public UserRegistrationException(List<string> errors)
            : base($"User registration failed: {string.Join(", ", errors)}") { }

        public UserRegistrationException(List<string> errors, Exception innerException)
            : base($"User registration failed: {string.Join(", ", errors)}", innerException)
        {
            
        }
    }
}
