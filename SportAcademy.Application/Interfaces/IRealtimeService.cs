namespace SportAcademy.Application.Interfaces;

public interface IRealtimeService
{
    Task AttendanceUpdated(int sessionOccurrenceId);
    Task SessionOccurrenceUpdated(int sessionOccurrenceId);
    Task EnrollmentUpdated(int enrollmentId);
    Task DashboardStatsUpdated();
    Task TraineeGroupUpdated(int traineeGroupId);
    Task SubscriptionUpdated(int subscriptionId);

    /// Pushed whenever the pending excuse-request queue changes (created, approved, or
    /// rejected) so the admin sidebar badge updates instantly instead of waiting on its poll.
    Task ExcuseRequestQueueUpdated();
}
