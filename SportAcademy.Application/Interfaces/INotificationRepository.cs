using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.NotificationsDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface INotificationRepository : IBaseRepository<Notification, int>
    {
        Task<Notification> AddWithRecipient(Notification notification, string userId);
        Task AddRecipientsForGroupAsync(int notificationId, string groupName, CancellationToken ct = default);
        Task<PagedData<NotificationRecipientDto>> GetUserNotificationsAsync(string userId, PageRequest page, CancellationToken ct = default);
        Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default);
        Task<bool> MarkAsReadAsync(int notificationId, string userId, CancellationToken ct = default);
        Task<int> MarkAllAsReadAsync(string userId, CancellationToken ct = default);
    }
}
