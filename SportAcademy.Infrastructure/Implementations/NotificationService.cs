using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
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
        private readonly INotificationChannelDispatcher _channelDispatcher;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IHubContext<NotificationHub, INotificationClient> hubContext,
            INotificationRepository notificationRepository,
            IUserRepository userRepository,
            ITenantIdProvider tenantIdProvider,
            INotificationChannelDispatcher channelDispatcher,
            ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _tenantIdProvider = tenantIdProvider;
            _channelDispatcher = channelDispatcher;
            _logger = logger;
        }

        /// MediatR's default IPublisher awaits every INotificationHandler in turn and lets an
        /// unhandled exception propagate straight back to whoever raised the event - which, for
        /// most of these 22 handlers, is a business command still mid-request (recording a
        /// payment, creating a subscription, ...). The InApp push above has already succeeded by
        /// the time this runs; a DB hiccup in the channel-routing tables must not take the
        /// triggering business operation down with it, so this is deliberately swallow-and-log,
        /// not rethrown. Worst case on failure: that one notification's Email/Push/WhatsApp
        /// queuing is silently skipped - InApp still delivered, and the next event for this
        /// recipient dispatches normally.
        private async Task DispatchChannelsSafelyAsync(int notificationId, IReadOnlyCollection<Guid> userIds, string eventType)
        {
            try
            {
                await _channelDispatcher.DispatchAsync(notificationId, userIds, eventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Channel dispatch failed for notification {NotificationId} (event {EventType}) - InApp delivery is unaffected.",
                    notificationId, eventType);
            }
        }

        public async Task BroadcastNotificationAsync(string eventType, string title, string message,
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

            // No per-recipient channel dispatch here: unlike every other send path, Broadcast
            // never persists individual NotificationRecipient rows to build a recipient list
            // from - there's nothing to resolve a destination or check a preference against.
            // (Unused by any current caller - see INotificationService's remarks.)
        }

        public async Task SendNotificationAsync(string eventType, string userId, string title, string message,
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

            await DispatchChannelsSafelyAsync(notification.Id, [Guid.Parse(userId)], eventType);
        }

        public async Task SendNotificationToGroupAsync(string eventType, string groupName, string title, string message,
            NotificationType type = NotificationType.System)
            => await SendNotificationToGroupsAsync(eventType, [groupName], title, message, type);

        public async Task SendNotificationToGroupsAsync(string eventType, IEnumerable<string> groupNames, string title, string message,
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
            await SendToUserIdsAsync(eventType, recipientIds, title, message, type, groupLabel);
        }

        public async Task SendNotificationToUsersAsync(string eventType, IEnumerable<Guid> userIds, string title, string message,
            NotificationType type = NotificationType.System)
        {
            var ids = userIds.Distinct().ToHashSet();
            if (ids.Count == 0) return;

            await SendToUserIdsAsync(eventType, ids, title, message, type);
        }

        /// Persists the notification, fans out a recipient row per user, pushes live to each of
        /// them by user id (Clients.Users) - independent of which SignalR "groups" (if any)
        /// their connection has joined, so it works whether or not the hub's own group
        /// bookkeeping is in sync - and dispatches the tenant's configured Email/Push/WhatsApp
        /// channels for this event on top of the InApp push above.
        private async Task SendToUserIdsAsync(
            string eventType, IReadOnlyCollection<Guid> userIds, string title, string message, NotificationType type, string? groupName = null)
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

            await DispatchChannelsSafelyAsync(notification.Id, userIds, eventType);
        }

        private async Task<List<Guid>> ResolveRoleGroupMemberIdsAsync(string groupName) => groupName switch
        {
            NotificationGroupNames.Admins => await _userRepository.GetUserIdsInRolesAsync(["Admin"]),
            NotificationGroupNames.Owners => await _userRepository.GetUserIdsInRolesAsync(["Owner"]),
            NotificationGroupNames.Employees => await _userRepository.GetEmployeeUserIdsAsync(),
            NotificationGroupNames.Accountants => await _userRepository.GetUserIdsInRolesAsync(["Accountant"]),
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
