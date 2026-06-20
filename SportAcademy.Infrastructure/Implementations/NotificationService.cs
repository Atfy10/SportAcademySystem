using Microsoft.AspNetCore.SignalR;
using SportAcademy.Application.DTOs.NotificationsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Notifications;

namespace SportAcademy.Infrastructure.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(IHubContext<NotificationHub, INotificationClient> hubContext,
            INotificationRepository notificationRepository)
        {
            _hubContext = hubContext;
            _notificationRepository = notificationRepository;
        }

        public async Task BroadcastNotificationAsync(string title, string message,
            NotificationType type = NotificationType.System)
        {
            var notification = new Notification
            {
                Title = title,
                Message = message,
                Type = type
            };
            await _notificationRepository.AddAsync(notification);

            await _hubContext.Clients.All.ReceiveNotification(new NotificationRecipientDto
            {
                Id = 0,
                Title = title,
                Message = message,
                Type = type,
                ActionUrl = null,
                IsRead = false,
                CreatedAt = notification.CreatedAt
            });
        }

        public async Task SendNotificationAsync(string userId, string title, string message,
            NotificationType type = NotificationType.System, string? actionUrl = null)
        {
            var notification = await _notificationRepository.AddWithRecipient(
                new Notification
                {
                    Title = title,
                    Message = message,
                    Type = type,
                    ActionUrl = actionUrl
                },
                userId);

            await _hubContext.Clients.User(userId).ReceiveNotification(new NotificationRecipientDto
            {
                Id = notification.Id,
                Title = notification.Title ?? title,
                Message = notification.Message,
                Type = notification.Type ?? type,
                ActionUrl = notification.ActionUrl,
                IsRead = false,
                CreatedAt = notification.CreatedAt
            });
        }

        public async Task SendNotificationToGroupAsync(string groupName, string title, string message,
            NotificationType type = NotificationType.System)
        {
            var notification = new Notification
            {
                Title = title,
                Message = message,
                Type = type,
                GroupName = groupName
            };
            await _notificationRepository.AddAsync(notification);

            await _hubContext.Clients.Group(groupName).ReceiveNotification(new NotificationRecipientDto
            {
                Id = 0,
                Title = title,
                Message = message,
                Type = type,
                ActionUrl = null,
                IsRead = false,
                CreatedAt = notification.CreatedAt
            });
        }
    }
}
