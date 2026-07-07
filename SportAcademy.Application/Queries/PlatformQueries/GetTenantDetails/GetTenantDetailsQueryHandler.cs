using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.PlatformQueries.GetTenantDetails;

public class GetTenantDetailsQueryHandler : IRequestHandler<GetTenantDetailsQuery, Result<TenantDetailResponse>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetTenantDetailsQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<TenantDetailResponse>> Handle(GetTenantDetailsQuery request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetDetailByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result<TenantDetailResponse>.Failure(_operation, "Tenant not found.", 404);

        var userCountTask = _tenantRepository.GetUserCountByTenantAsync(request.TenantId, ct);
        var branchCountTask = _tenantRepository.GetBranchCountByTenantAsync(request.TenantId, ct);
        var sportCountTask = _tenantRepository.GetSportCountByTenantAsync(request.TenantId, ct);
        await Task.WhenAll(userCountTask, branchCountTask, sportCountTask);

        return Result<TenantDetailResponse>.Success(
            tenant.ToDetailResponse(userCountTask.Result, branchCountTask.Result, sportCountTask.Result),
            _operation);
    }
}
