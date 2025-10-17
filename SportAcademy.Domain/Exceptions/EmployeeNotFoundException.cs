using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
    public class EmployeeNotFoundException : IdNotFoundException
    {
        static readonly string _entity = nameof(Employee);

        public EmployeeNotFoundException(string id) : base(_entity, id) { }

        public EmployeeNotFoundException(string id, Exception innerException) 
            : base(_entity, id, innerException) { }

    }
}
