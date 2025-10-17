using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
    public class SportTraineeNotFoundException : IdNotFoundException
    {
        static readonly string _entity = nameof(SportTrainee);

        public SportTraineeNotFoundException(string id) : base(_entity, id)
        {

        }
        public SportTraineeNotFoundException(string id, Exception innerException) 
            : base(_entity, id, innerException)
        {

        }
    }
}
