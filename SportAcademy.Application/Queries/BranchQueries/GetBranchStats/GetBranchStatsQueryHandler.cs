using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.BranchDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.BranchQueries.GetBranchStats;

public class GetBranchStatsQueryHandler : IRequestHandler<GetBranchStatsQuery, Result<BranchStatsDto>>
{
    private readonly IBranchRepository _branchRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetBranchStatsQueryHandler(IBranchRepository branchRepository)
    {
        _branchRepository = branchRepository;
    }

    public async Task<Result<BranchStatsDto>> Handle(GetBranchStatsQuery request, CancellationToken cancellationToken)
    {
        var stats = await _branchRepository.GetBranchStatsAsync(request.BranchId, cancellationToken);
        return Result<BranchStatsDto>.Success(stats, _operation);
    }
}
