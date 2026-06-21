using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetStats
{
    public class GetSubDetailsStatsQueryHandler : IRequestHandler<GetSubDetailsStatsQuery, Result<SubscriptionStatsDto>>
    {
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;
        private readonly string _operationType = OperationType.GetAll.ToString();

        public GetSubDetailsStatsQueryHandler(ISubscriptionDetailsRepository subscriptionDetailsRepository)
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
        }

        public async Task<Result<SubscriptionStatsDto>> Handle(GetSubDetailsStatsQuery request, CancellationToken cancellationToken)
        {
            var stats = await _subscriptionDetailsRepository.GetSubDetailsStatsAsync(cancellationToken);
            return Result<SubscriptionStatsDto>.Success(stats, _operationType);
        }
    }
}
