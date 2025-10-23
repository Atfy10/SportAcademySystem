using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
    public class EnrollmentNotFoundException : IdNotFoundException
    {
        static readonly string _entity = nameof(Entities.Enrollment);
        public EnrollmentNotFoundException(string id) : base(_entity, id) { }
        public EnrollmentNotFoundException(string id, Exception innerException)
            : base(_entity, id, innerException) { }
    }
}
