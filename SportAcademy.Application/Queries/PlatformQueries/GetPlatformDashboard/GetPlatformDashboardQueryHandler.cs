using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.PlatformQueries.GetPlatformDashboard;

public class GetPlatformDashboardQueryHandler : IRequestHandler<GetPlatformDashboardQuery, Result<PlatformDashboardResponse>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetPlatformDashboardQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<PlatformDashboardResponse>> Handle(GetPlatformDashboardQuery request, CancellationToken ct)
    {
        var totalTenants = await _tenantRepository.GetCountAsync(ct);
        var statusCounts = await _tenantRepository.GetStatusCountsAsync(ct);
        var totalUsers = await _tenantRepository.GetTotalUsersAsync(ct);
        var totalBranches = await _tenantRepository.GetTotalBranchesAsync(ct);

        var dashboard = PlatformMapper.ToDashboardResponse(
            totalTenants, statusCounts, totalUsers, totalBranches);

        return Result<PlatformDashboardResponse>.Success(dashboard, _operation);
    }
}
