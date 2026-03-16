using SportAcademy.Application.Common.Pagination;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface INotificationRepository : IBaseRepository<Notification, int>
    {
        Task AddWithRecipient(Notification notification, string userId);
        Task<PagedData<NotificationRecipientDto>> GetUserNotificationsAsync(string userId, PageRequest page, CancellationToken ct = default);
        Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default);
        Task<bool> MarkAsReadAsync(int notificationId, string userId, CancellationToken ct = default);
        Task<int> MarkAllAsReadAsync(string userId, CancellationToken ct = default);
    }

    public class NotificationRecipientDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "system";
        public string? ActionUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
