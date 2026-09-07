using SportAcademy.Application.DTOs.NotificationsDtos;

namespace SportAcademy.Application.Interfaces
{
    public interface INotificationClient
    {
        Task ReceiveNotification(NotificationRecipientDto notification);
        Task NotificationRead(int notificationId);
        Task AllNotificationsRead();
        Task AttendanceUpdated(int sessionOccurrenceId);
        Task SessionOccurrenceUpdated(int sessionOccurrenceId);
        Task EnrollmentUpdated(int enrollmentId);
        Task DashboardStatsUpdated();
        Task TraineeGroupUpdated(int traineeGroupId);
        Task SubscriptionUpdated(int subscriptionId);
        Task ExcuseRequestQueueUpdated();

        /// Pushed to every connection in the tenant's group the moment a SuperAdmin moves it
        /// away from Active (suspend/archive) - the client must stop the connection and sign
        /// out immediately rather than waiting for the next API call to hit
        /// TenantStatusGuardMiddleware's 403. newStatus is the raw TenantStatus enum name
        /// (e.g. "Suspended", "Archived").
        Task TenantStatusChanged(string newStatus);
    }
}
