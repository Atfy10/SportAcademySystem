using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Domain.Exceptions.EmployeeExceptions
{
    public class EmployeeAlreadyCoachException : LocalizableException
    {
        public EmployeeAlreadyCoachException()
            : base(
                "errors.employee.alreadyCoach",
                "This employee already has an active coach record.")
        {
        }
    }
}
