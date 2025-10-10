using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SportAcademy.Domain.Exceptions
{
    public class SessionDateMismatchException : Exception
    {
        private const string _message = "Attendance can only be taken on the session date.";

        public SessionDateMismatchException()
            : base(_message)
        {
        }
        public SessionDateMismatchException(Exception inner)
            : base(_message, inner)
        {
        }
    }
}
