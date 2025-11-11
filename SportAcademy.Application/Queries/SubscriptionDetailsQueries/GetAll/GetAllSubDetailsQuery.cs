using MediatR;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.Services;

namespace SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetAll
{
    public record GetAllSubDetailsQuery() : IRequest<Result<List<SubscriptionDetailsDto>>>;
}
