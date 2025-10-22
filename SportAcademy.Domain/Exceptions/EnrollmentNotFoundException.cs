using SportAcademy.Domain.Entities;

namespace SportAcademy.Domain.Exceptions
{
    public class EnrollmentNotFoundException : IdNotFoundException
    {
        static readonly string _entity = nameof(Enrollment);
        public EnrollmentNotFoundException(string id) : base(_entity, id) { }
        public EnrollmentNotFoundException(string id, Exception innerException)
            : base(_entity, id, innerException) { }
    }
}
