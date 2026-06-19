using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Queries.NotificationQueries.GetUnreadCount
{
    public record GetUnreadCountQuery : IRequest<Result<int>>;
}
