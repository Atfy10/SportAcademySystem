using MediatR;
using Microsoft.AspNetCore.SignalR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task BroadcastNotificationAsync(string message)
        {
            await _notificationRepository.AddAsync(new Notification { Message = message });
            await _hubContext.Clients.All.RecieveNotification(message);
        }

        public async Task SendNotificationAsync(string userId, string message)
        {
            await _notificationRepository.AddWithRecipient(new Notification { Message = message }, userId);
            await _hubContext.Clients.User(userId).RecieveNotification(message);
        }

        public async Task SendNotificationToGroupAsync(string groupName, string message)
        {
            await _notificationRepository.AddAsync(new Notification { Message = message, GroupName = groupName });
            await _hubContext.Clients.Group(groupName).RecieveNotification(message);
        }
    }
}
