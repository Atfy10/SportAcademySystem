using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.NotificationCommands.MarkAllNotificationsAsRead
{
    public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, int>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserContextService _userContext;
        private readonly INotificationService _notificationService;

        public MarkAllNotificationsAsReadCommandHandler(
            INotificationRepository notificationRepository,
            IUserContextService userContext,
            INotificationService notificationService)
        {
            _notificationRepository = notificationRepository;
            _userContext = userContext;
            _notificationService = notificationService;
        }

        public async Task<int> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;
            if (userId is null)
                return 0;

            var count = await _notificationRepository.MarkAllAsReadAsync(userId.Value, cancellationToken);

            if (count > 0)
                await _notificationService.NotifyAllNotificationsReadAsync(userId.Value.ToString());

            return count;
        }
    }
}
