using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.NotificationsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.NotificationQueries.GetUserNotifications
{
    public class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, Result<PagedData<NotificationRecipientDto>>>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserContextService _userContext;
        private readonly string _operation = OperationType.GetAll.ToString();

        public GetUserNotificationsQueryHandler(
            INotificationRepository notificationRepository,
            IUserContextService userContext)
        {
            _notificationRepository = notificationRepository;
            _userContext = userContext;
        }

        public async Task<Result<PagedData<NotificationRecipientDto>>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;
            if (userId is null)
                return Result<PagedData<NotificationRecipientDto>>.Failure(_operation, "User ID is not available in the context.", 400);

            var data = await _notificationRepository.GetUserNotificationsAsync(
                userId.Value,
                request.PageRequest,
                cancellationToken);

            return Result<PagedData<NotificationRecipientDto>>.Success(data, _operation);
        }
    }
}
