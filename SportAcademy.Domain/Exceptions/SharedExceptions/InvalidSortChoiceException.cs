using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions.SharedExceptions
{
    public class InvalidSortChoiceException : Exception
    {
        static readonly string _message = "The selected sorting option is not valid. Please choose a valid option.";

        public InvalidSortChoiceException() : base(_message)
        {

        }
        public InvalidSortChoiceException(Exception innerException) : base(_message, innerException)
        {

        }

    }
}
