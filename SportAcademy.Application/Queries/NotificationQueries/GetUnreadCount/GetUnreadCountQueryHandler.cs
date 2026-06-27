using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos;
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
            var userId = _userContext.UserId;
            if (userId is null)
                return Result<int>.Failure(_operation, "User ID is not available in the context.", 400);

            var count = await _notificationRepository.GetUnreadCountAsync(userId.Value, cancellationToken);
            return Result<int>.Success(count, _operation);
        }
    }
}
