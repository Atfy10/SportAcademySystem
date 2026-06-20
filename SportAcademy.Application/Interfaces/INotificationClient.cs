using SportAcademy.Application.DTOs.NotificationsDtos;

namespace SportAcademy.Application.Interfaces
{
    public interface INotificationClient
    {
        Task ReceiveNotification(NotificationRecipientDto notification);
        Task AttendanceUpdated(int sessionOccurrenceId);
        Task SessionOccurrenceUpdated(int sessionOccurrenceId);
        Task EnrollmentUpdated(int enrollmentId);
        Task DashboardStatsUpdated();
        Task TraineeGroupUpdated(int traineeGroupId);
    }
}
