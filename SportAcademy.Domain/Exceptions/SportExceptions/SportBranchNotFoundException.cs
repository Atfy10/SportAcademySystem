using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions.SportExceptions
{
    public class SportBranchNotFoundException : Exception
    {
        static readonly string _message = "This sport is not offered at the selected branch.";

        public SportBranchNotFoundException() : base(_message)
        {

        }
        public SportBranchNotFoundException(Exception innerException) : base(_message, innerException)
        {

        }

    }
}
