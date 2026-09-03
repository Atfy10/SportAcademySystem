using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.ExcuseRequestDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.ExcuseRequestQueries.GetPending;

public class GetPendingExcuseRequestsQueryHandler(
    IExcuseRequestRepository excuseRequestRepository)
    : IRequestHandler<GetPendingExcuseRequestsQuery, Result<PagedData<ExcuseRequestDto>>>
{
    public async Task<Result<PagedData<ExcuseRequestDto>>> Handle(
        GetPendingExcuseRequestsQuery request, CancellationToken cancellationToken)
    {
        var items = await excuseRequestRepository.GetPendingAsync(request.Page, cancellationToken);
        return Result<PagedData<ExcuseRequestDto>>.Success(items, OperationType.GetAll.ToString());
    }
}
