using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.CoachQueries.GetCoachBranches;

public class GetCoachBranchesQueryHandler : IRequestHandler<GetCoachBranchesQuery, Result<List<int>>>
{
    private readonly ICoachBranchAccessRepository _coachBranchAccessRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetCoachBranchesQueryHandler(ICoachBranchAccessRepository coachBranchAccessRepository)
    {
        _coachBranchAccessRepository = coachBranchAccessRepository;
    }

    public async Task<Result<List<int>>> Handle(GetCoachBranchesQuery request, CancellationToken ct)
    {
        var access = await _coachBranchAccessRepository.GetForCoachAsync(request.CoachId, ct);
        return Result<List<int>>.Success(access.Select(a => a.BranchId).ToList(), _operation);
    }
}
