using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionTypeDtos;

namespace SportAcademy.Application.Queries.SubscriptionTypeQueries.GetById
{
    public record GetSubscriptionTypeByIdQuery(int Id) : IRequest<Result<SubscriptionTypeDto>>;
}