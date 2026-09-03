using Microsoft.AspNetCore.SignalR;
using SportAcademy.Application.DTOs.NotificationsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Helpers;
using SportAcademy.Infrastructure.Notifications;

namespace SportAcademy.Infrastructure.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITenantIdProvider _tenantIdProvider;

        public NotificationService(IHubContext<NotificationHub, INotificationClient> hubContext,
            INotificationRepository notificationRepository,
            IUserRepository userRepository,
            ITenantIdProvider tenantIdProvider)
        {
            _hubContext = hubContext;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
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
            => await SendNotificationToGroupsAsync([groupName], title, message, type);

        public async Task SendNotificationToGroupsAsync(IEnumerable<string> groupNames, string title, string message,
            NotificationType type = NotificationType.System, IEnumerable<Guid>? extraUserIds = null)
        {
            var names = groupNames.Distinct().ToList();

            // Every named group's membership is resolved live from role/employment data (never
            // NotificationGroupMembers, a connection-time cache) so this always reaches every
            // current member regardless of SignalR connection history - see
            // ResolveRoleGroupMemberIdsAsync.
            var recipientIds = new HashSet<Guid>();
            foreach (var name in names)
            {
                foreach (var id in await ResolveRoleGroupMemberIdsAsync(name))
                {
                    recipientIds.Add(id);
                }
            }

            if (extraUserIds is not null)
            {
                foreach (var id in extraUserIds)
                {
                    recipientIds.Add(id);
                }
            }

            if (recipientIds.Count == 0) return;

            var groupLabel = ScopedGroup(string.Join("+", names));
            await SendToUserIdsAsync(recipientIds, title, message, type, groupLabel);
        }

        public async Task SendNotificationToUsersAsync(IEnumerable<Guid> userIds, string title, string message,
            NotificationType type = NotificationType.System)
        {
            var ids = userIds.Distinct().ToHashSet();
            if (ids.Count == 0) return;

            await SendToUserIdsAsync(ids, title, message, type);
        }

        /// Persists the notification, fans out a recipient row per user, and pushes live to
        /// each of them by user id (Clients.Users) - independent of which SignalR "groups" (if
        /// any) their connection has joined, so it works whether or not the hub's own group
        /// bookkeeping is in sync.
        private async Task SendToUserIdsAsync(
            IReadOnlyCollection<Guid> userIds, string title, string message, NotificationType type, string? groupName = null)
        {
            var notification = new Notification
            {
                Title = title,
                Message = message,
                Type = type,
                GroupName = groupName
            };
            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.AddRecipientsForUsersAsync(notification.Id, userIds);

            await _hubContext.Clients.Users(userIds.Select(id => id.ToString()).ToList()).ReceiveNotification(new NotificationRecipientDto
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

        private async Task<List<Guid>> ResolveRoleGroupMemberIdsAsync(string groupName) => groupName switch
        {
            NotificationGroupNames.Admins => await _userRepository.GetUserIdsInRolesAsync(["Admin"]),
            NotificationGroupNames.Owners => await _userRepository.GetUserIdsInRolesAsync(["Owner"]),
            NotificationGroupNames.Employees => await _userRepository.GetEmployeeUserIdsAsync(),
            _ => [],
        };

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
