using MediatR;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.LimitDtos;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.PlatformQueries.GetTenantLimits;

public class GetTenantLimitsQueryHandler : IRequestHandler<GetTenantLimitsQuery, Result<List<TenantLimitResponse>>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IEffectiveLimitService _limitService;
    private readonly string _operation = OperationType.Get.ToString();

    public GetTenantLimitsQueryHandler(ITenantRepository tenantRepository, IEffectiveLimitService limitService)
    {
        _tenantRepository = tenantRepository;
        _limitService = limitService;
    }

    public async Task<Result<List<TenantLimitResponse>>> Handle(GetTenantLimitsQuery request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result<List<TenantLimitResponse>>.Failure(_operation, "Tenant not found.", 404);

        var limits = await _limitService.GetAllAsync(request.TenantId, ct);

        var response = limits.Select(l => new TenantLimitResponse
        {
            ResourceKey = l.ResourceKey,
            MaxCount = l.MaxCount,
            Used = l.Used,
            Source = l.Source.ToString(),
            OverrideReason = l.OverrideReason,
            IsOverLimit = l.IsOverLimit,
        }).ToList();

        return Result<List<TenantLimitResponse>>.Success(response, _operation);
    }
}
