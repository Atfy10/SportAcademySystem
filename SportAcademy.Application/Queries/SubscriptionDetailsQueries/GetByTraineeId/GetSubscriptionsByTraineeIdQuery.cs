using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;

namespace SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetByTraineeId
{
    public record GetSubscriptionsByTraineeIdQuery(int TraineeId) : IRequest<Result<List<SubscriptionDetailsDto>>>;
}
