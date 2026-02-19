using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;

namespace SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetAll
{
    public record GetAllSubDetailsQuery() : IRequest<Result<List<SubscriptionDetailsDto>>>;
}
