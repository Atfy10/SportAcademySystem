using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.NotificationsDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.NotificationQueries.GetUserNotifications
{
    public class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, PagedData<NotificationRecipientDto>>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserContextService _userContext;

        public GetUserNotificationsQueryHandler(
            INotificationRepository notificationRepository,
            IUserContextService userContext)
        {
            _notificationRepository = notificationRepository;
            _userContext = userContext;
        }

        public async Task<PagedData<NotificationRecipientDto>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
        {
            return await _notificationRepository.GetUserNotificationsAsync(
                _userContext.UserId,
                request.PageRequest,
                cancellationToken);
        }
    }
}
