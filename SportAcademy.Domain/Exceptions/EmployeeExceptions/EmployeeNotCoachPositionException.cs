using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Domain.Exceptions.EmployeeExceptions
{
    public class EmployeeNotCoachPositionException : LocalizableException
    {
        public EmployeeNotCoachPositionException(string currentPosition)
            : base(
                "errors.employee.notCoachPosition",
                $"This employee's position is \"{currentPosition}\", not Coach. Update their position to Coach before creating a coach record for them.",
                currentPosition)
        {
        }
    }
}
