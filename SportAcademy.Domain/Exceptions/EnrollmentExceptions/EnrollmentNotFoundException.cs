using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Domain.Exceptions.EnrollmentExceptions
{
    public class EnrollmentNotFoundException : IdNotFoundException
    {
        static readonly string _entity = nameof(Enrollment);
        public EnrollmentNotFoundException(string id) : base(_entity, id) { }
        public EnrollmentNotFoundException(string id, Exception innerException)
            : base(_entity, id, innerException) { }
    }
}
