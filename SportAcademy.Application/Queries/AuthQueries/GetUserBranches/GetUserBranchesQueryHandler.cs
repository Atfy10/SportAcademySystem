using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.AuthQueries.GetUserBranches;

public class GetUserBranchesQueryHandler : IRequestHandler<GetUserBranchesQuery, Result<List<int>>>
{
    private readonly IUserBranchAccessRepository _userBranchAccessRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetUserBranchesQueryHandler(IUserBranchAccessRepository userBranchAccessRepository)
    {
        _userBranchAccessRepository = userBranchAccessRepository;
    }

    public async Task<Result<List<int>>> Handle(GetUserBranchesQuery request, CancellationToken ct)
    {
        var access = await _userBranchAccessRepository.GetForUserAsync(request.UserId, ct);
        return Result<List<int>>.Success(access.Select(a => a.BranchId).ToList(), _operation);
    }
}
