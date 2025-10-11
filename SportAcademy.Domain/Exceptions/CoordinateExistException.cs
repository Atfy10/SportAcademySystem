using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
    public class CoordinateExistException : Exception
    {
        static readonly string _message = "This branch has already been registered. Please choose another branch.";
        public CoordinateExistException() : base(_message) { }
        public CoordinateExistException(Exception innerException) : base(_message, innerException)
        {

        }
    }
}
