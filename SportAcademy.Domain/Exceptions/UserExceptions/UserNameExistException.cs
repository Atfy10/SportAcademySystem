using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions.UserExceptions
{
    public class UserNameExistException : Exception
    {
        static readonly string _message = "This username is already taken. Please choose a different one.";

        public UserNameExistException() : base(_message)
        {

        }
        public UserNameExistException(Exception innerException) : base(_message, innerException)
        {

        }

    }
}
