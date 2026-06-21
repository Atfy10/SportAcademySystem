using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;

namespace SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetStats
{
    public record GetSubDetailsStatsQuery() : IRequest<Result<SubscriptionStatsDto>>;
}
