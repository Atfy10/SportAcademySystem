using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Domain.Exceptions.EmployeeExceptions
{
    // Thrown when an employee's position is being changed away from Coach but the coach record
    // can't be removed because trainee groups still reference it. groupNames is already
    // truncated/joined by the caller so the message stays readable for coaches with many groups.
    public class CoachHasGroupsException : LocalizableException
    {
        public CoachHasGroupsException(int groupCount, string groupNames)
            : base(
                "errors.employee.coachHasGroups",
                $"This employee is still the coach of {groupCount} group(s): {groupNames}. Assign those groups to another coach before changing their position.",
                groupCount,
                groupNames)
        {
        }
    }
}
