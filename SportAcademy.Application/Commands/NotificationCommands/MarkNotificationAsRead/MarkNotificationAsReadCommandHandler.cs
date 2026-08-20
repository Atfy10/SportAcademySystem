using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.NotificationCommands.MarkNotificationAsRead
{
    public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, bool>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserContextService _userContext;
        private readonly INotificationService _notificationService;

        public MarkNotificationAsReadCommandHandler(
            INotificationRepository notificationRepository,
            IUserContextService userContext,
            INotificationService notificationService)
        {
            _notificationRepository = notificationRepository;
            _userContext = userContext;
            _notificationService = notificationService;
        }

        public async Task<bool> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;
            if (userId is null)
                return false;

            var marked = await _notificationRepository.MarkAsReadAsync(
                request.NotificationId,
                userId.Value,
                cancellationToken);

            if (marked)
                await _notificationService.NotifyNotificationReadAsync(userId.Value.ToString(), request.NotificationId);

            return marked;
        }
    }
}
