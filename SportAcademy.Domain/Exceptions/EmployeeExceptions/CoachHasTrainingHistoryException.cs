using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Domain.Exceptions.EmployeeExceptions
{
    // Thrown when an employee's position is being changed away from Coach but trainees' coaching
    // history (TraineeCareerEvent) still points at the coach record - deleting it would erase
    // that history, so the position has to stay Coach (and the employee can be deactivated).
    public class CoachHasTrainingHistoryException : LocalizableException
    {
        public CoachHasTrainingHistoryException(int eventCount)
            : base(
                "errors.employee.coachHasTrainingHistory",
                $"This employee can't be moved out of the Coach position because {eventCount} trainee coaching history record(s) refer to them. Keep their position as Coach and deactivate them instead if they no longer work here.",
                eventCount)
        {
        }
    }
}
