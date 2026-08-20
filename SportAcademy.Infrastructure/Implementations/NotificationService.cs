using Microsoft.AspNetCore.SignalR;
using SportAcademy.Application.DTOs.NotificationsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Notifications;

namespace SportAcademy.Infrastructure.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
        private readonly INotificationRepository _notificationRepository;
        private readonly ITenantIdProvider _tenantIdProvider;

        public NotificationService(IHubContext<NotificationHub, INotificationClient> hubContext,
            INotificationRepository notificationRepository,
            ITenantIdProvider tenantIdProvider)
        {
            _hubContext = hubContext;
            _notificationRepository = notificationRepository;
            _tenantIdProvider = tenantIdProvider;
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

            // Scoped to the current tenant's "General" group - a broadcast is "everyone in
            // this academy", never every client of every academy on the platform.
            await _hubContext.Clients.Group(GeneralGroup()).ReceiveNotification(new NotificationRecipientDto
            {
                Id = notification.Id,
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
                Guid.Parse(userId));

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
            // The group name is scoped to the current tenant once, here, so the SignalR
            // broadcast and the persisted NotificationGroupMember lookup always agree - and so
            // an "Admins" notification can never be delivered to another tenant's admins.
            var scopedGroupName = ScopedGroup(groupName);

            var notification = new Notification
            {
                Title = title,
                Message = message,
                Type = type,
                GroupName = scopedGroupName
            };
            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.AddRecipientsForGroupAsync(notification.Id, scopedGroupName);

            await _hubContext.Clients.Group(scopedGroupName).ReceiveNotification(new NotificationRecipientDto
            {
                Id = notification.Id,
                Title = title,
                Message = message,
                Type = type,
                ActionUrl = null,
                IsRead = false,
                CreatedAt = notification.CreatedAt
            });
        }

        public async Task NotifyNotificationReadAsync(string userId, int notificationId)
            => await _hubContext.Clients.User(userId).NotificationRead(notificationId);

        public async Task NotifyAllNotificationsReadAsync(string userId)
            => await _hubContext.Clients.User(userId).AllNotificationsRead();

        private string GeneralGroup() => ScopedGroup(NotificationGroupNames.General);

        private string ScopedGroup(string baseName)
        {
            var tenantId = _tenantIdProvider.TenantId
                ?? throw new InvalidOperationException("NotificationService invoked without a resolved tenant context.");

            return NotificationGroupNames.ForTenant(tenantId, baseName);
        }
    }
}
