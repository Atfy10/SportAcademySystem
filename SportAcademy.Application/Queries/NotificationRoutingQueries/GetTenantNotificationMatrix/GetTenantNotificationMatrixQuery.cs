using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.NotificationsDtos;

namespace SportAcademy.Application.Queries.NotificationRoutingQueries.GetTenantNotificationMatrix;

public record GetTenantNotificationMatrixQuery : IRequest<Result<List<NotificationMatrixCellDto>>>;
