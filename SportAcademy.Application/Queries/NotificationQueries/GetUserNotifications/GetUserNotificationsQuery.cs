using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.NotificationsDtos;

namespace SportAcademy.Application.Queries.NotificationQueries.GetUserNotifications
{
    public record GetUserNotificationsQuery(
        PageRequest PageRequest) : IRequest<Result<PagedData<NotificationRecipientDto>>>;
}
