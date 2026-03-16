using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.NotificationsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class NotificationRepository : BaseRepository<Notification, int>, INotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public NotificationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task AddWithRecipient(Notification notification, string userId)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.NotificationRecipients.AddAsync(new NotificationRecipient
            {
                Notification = notification,
                UserId = userId,
                IsRead = false
            });

            await SaveChanges();
        }

        public async Task<PagedData<NotificationRecipientDto>> GetUserNotificationsAsync(
            string userId, PageRequest page, CancellationToken ct = default)
        {
            var query = _context.NotificationRecipients
                .AsNoTracking()
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Notification.CreatedAt)
                .Select(r => new NotificationRecipientDto
                {
                    Id = r.NotificationId,
                    Title = r.Notification.GroupName ?? "Notification",
                    Message = r.Notification.Message,
                    Type = r.Notification.GroupName != null ? "group" : "system",
                    IsRead = r.IsRead,
                    CreatedAt = r.Notification.CreatedAt
                });

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .Skip(page.Skip)
                .Take(page.PageSize)
                .ToListAsync(ct);

            return new PagedData<NotificationRecipientDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page.Page,
                PageSize = page.PageSize
            };
        }

        public async Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default)
        {
            return await _context.NotificationRecipients
                .AsNoTracking()
                .Where(r => r.UserId == userId && !r.IsRead)
                .CountAsync(ct);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, string userId, CancellationToken ct = default)
        {
            var recipient = await _context.NotificationRecipients
                .FirstOrDefaultAsync(r => r.NotificationId == notificationId && r.UserId == userId, ct);

            if (recipient == null) return false;

            recipient.IsRead = true;
            await SaveChanges(ct);
            return true;
        }

        public async Task<int> MarkAllAsReadAsync(string userId, CancellationToken ct = default)
        {
            return await _context.NotificationRecipients
                .Where(r => r.UserId == userId && !r.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsRead, true), ct);
        }
    }
}
