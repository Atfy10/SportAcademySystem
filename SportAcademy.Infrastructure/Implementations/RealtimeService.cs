using Microsoft.AspNetCore.SignalR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Infrastructure.Notifications;

namespace SportAcademy.Infrastructure.Implementations;

public class RealtimeService : IRealtimeService
{
    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;

    public RealtimeService(IHubContext<NotificationHub, INotificationClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task AttendanceUpdated(int sessionOccurrenceId)
        => await _hubContext.Clients.All.AttendanceUpdated(sessionOccurrenceId);

    public async Task SessionOccurrenceUpdated(int sessionOccurrenceId)
        => await _hubContext.Clients.All.SessionOccurrenceUpdated(sessionOccurrenceId);

    public async Task EnrollmentUpdated(int enrollmentId)
        => await _hubContext.Clients.All.EnrollmentUpdated(enrollmentId);

    public async Task DashboardStatsUpdated()
        => await _hubContext.Clients.All.DashboardStatsUpdated();

    public async Task TraineeGroupUpdated(int traineeGroupId)
        => await _hubContext.Clients.All.TraineeGroupUpdated(traineeGroupId);

    public async Task SubscriptionUpdated(int subscriptionId)
        => await _hubContext.Clients.All.SubscriptionUpdated(subscriptionId);
}
