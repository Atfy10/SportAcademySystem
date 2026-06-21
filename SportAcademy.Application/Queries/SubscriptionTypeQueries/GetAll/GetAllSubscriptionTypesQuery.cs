using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionTypeDtos;

namespace SportAcademy.Application.Queries.SubscriptionTypeQueries.GetAll
{
    public record GetAllSubscriptionTypesQuery() : IRequest<Result<List<SubscriptionTypeDto>>>;
}