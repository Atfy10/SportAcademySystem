using MediatR;

namespace SportAcademy.Application.Queries.NotificationQueries.GetUnreadCount
{
    public record GetUnreadCountQuery : IRequest<int>;
}
