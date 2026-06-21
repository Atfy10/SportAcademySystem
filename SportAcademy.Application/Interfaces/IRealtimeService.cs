namespace SportAcademy.Application.Interfaces;

public interface IRealtimeService
{
    Task AttendanceUpdated(int sessionOccurrenceId);
    Task SessionOccurrenceUpdated(int sessionOccurrenceId);
    Task EnrollmentUpdated(int enrollmentId);
    Task DashboardStatsUpdated();
    Task TraineeGroupUpdated(int traineeGroupId);
    Task SubscriptionUpdated(int subscriptionId);
}
