using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.NotificationCommands.MarkNotificationAsRead
{
    public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Result>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserContextService _userContext;
        private readonly INotificationService _notificationService;
        private readonly string _operation = OperationType.Update.ToString();

        public MarkNotificationAsReadCommandHandler(
            INotificationRepository notificationRepository,
            IUserContextService userContext,
            INotificationService notificationService)
        {
            _notificationRepository = notificationRepository;
            _userContext = userContext;
            _notificationService = notificationService;
        }

        public async Task<Result> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;
            if (userId is null)
                return Result.Failure(_operation, "User is not available.", 400);

            var marked = await _notificationRepository.MarkAsReadAsync(
                request.NotificationId,
                userId.Value,
                cancellationToken);

            if (!marked)
                return Result.Failure(_operation, "Notification not found.", 404);

            await _notificationService.NotifyNotificationReadAsync(userId.Value.ToString(), request.NotificationId);

            return Result.Success(_operation, "Notification marked as read.");
        }
    }
}
