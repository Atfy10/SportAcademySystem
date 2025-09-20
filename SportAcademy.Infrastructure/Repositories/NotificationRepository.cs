using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Infrastructure.Repositories
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
    }
}
