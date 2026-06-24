using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.NotificationsDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface INotificationRepository : IBaseRepository<Notification, int>
    {
        Task<Notification> AddWithRecipient(Notification notification, Guid userId);
        Task AddRecipientsForGroupAsync(int notificationId, string groupName, CancellationToken ct = default);
        Task<PagedData<NotificationRecipientDto>> GetUserNotificationsAsync(Guid userId, PageRequest page, CancellationToken ct = default);
        Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
        Task<bool> MarkAsReadAsync(int notificationId, Guid userId, CancellationToken ct = default);
        Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);
    }
}
