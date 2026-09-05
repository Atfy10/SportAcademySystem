using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.CoachDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.CoachQueries.GetCoachBranches;

public class GetCoachBranchesQueryHandler : IRequestHandler<GetCoachBranchesQuery, Result<List<CoachBranchAccessDto>>>
{
    private readonly ICoachBranchAccessRepository _coachBranchAccessRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetCoachBranchesQueryHandler(ICoachBranchAccessRepository coachBranchAccessRepository)
    {
        _coachBranchAccessRepository = coachBranchAccessRepository;
    }

    public async Task<Result<List<CoachBranchAccessDto>>> Handle(GetCoachBranchesQuery request, CancellationToken ct)
    {
        var access = await _coachBranchAccessRepository.GetForCoachAsync(request.CoachId, ct);
        var dtos = access
            .Select(a => new CoachBranchAccessDto(a.BranchId, a.Branch.Name, a.Salary))
            .ToList();
        return Result<List<CoachBranchAccessDto>>.Success(dtos, _operation);
    }
}
