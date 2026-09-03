using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.ExcuseRequestQueries.CountPending;

public class CountPendingExcuseRequestsQueryHandler(
    IExcuseRequestRepository excuseRequestRepository)
    : IRequestHandler<CountPendingExcuseRequestsQuery, Result<int>>
{
    public async Task<Result<int>> Handle(CountPendingExcuseRequestsQuery request, CancellationToken cancellationToken)
    {
        var count = await excuseRequestRepository.CountPendingAsync(cancellationToken);
        return Result<int>.Success(count, OperationType.Get.ToString());
    }
}
