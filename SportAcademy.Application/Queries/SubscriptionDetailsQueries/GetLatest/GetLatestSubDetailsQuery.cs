using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;

namespace SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetLatest
{
    public record GetLatestSubDetailsQuery() : IRequest<Result<List<SubscriptionDetailsDto>>>;
}
