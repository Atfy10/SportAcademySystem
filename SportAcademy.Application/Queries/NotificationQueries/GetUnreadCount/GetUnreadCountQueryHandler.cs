using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.NotificationQueries.GetUnreadCount
{
    public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, Result<int>>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserContextService _userContext;
        private readonly string _operation = OperationType.GetAll.ToString();

        public GetUnreadCountQueryHandler(
            INotificationRepository notificationRepository,
            IUserContextService userContext)
        {
            _notificationRepository = notificationRepository;
            _userContext = userContext;
        }

        public async Task<Result<int>> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
        {
            var count = await _notificationRepository.GetUnreadCountAsync(Guid.Parse(_userContext.UserId), cancellationToken);
            return Result<int>.Success(count, _operation);
        }
    }
}
