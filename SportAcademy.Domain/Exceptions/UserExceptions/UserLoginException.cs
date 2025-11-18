using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions.UserExceptions
{
    public class UserLoginException : Exception
    {
        private static readonly string _message = "User login failed.";

        public UserLoginException() : base(_message) { }

        public UserLoginException(Exception innerException)
            : base(_message, innerException) { }
    }
}
