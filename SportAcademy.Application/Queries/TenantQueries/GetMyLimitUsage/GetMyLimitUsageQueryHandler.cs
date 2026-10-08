using MediatR;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.LimitDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Queries.TenantQueries.GetMyLimitUsage;

public class GetMyLimitUsageQueryHandler : IRequestHandler<GetMyLimitUsageQuery, Result<List<TenantLimitResponse>>>
{
    private readonly IUserContextService _userContext;
    private readonly IEffectiveLimitService _limitService;
    private readonly string _operation = OperationType.Get.ToString();

    public GetMyLimitUsageQueryHandler(IUserContextService userContext, IEffectiveLimitService limitService)
    {
        _userContext = userContext;
        _limitService = limitService;
    }

    public async Task<Result<List<TenantLimitResponse>>> Handle(GetMyLimitUsageQuery request, CancellationToken ct)
    {
        // TenantResolutionMiddleware already 400s an authenticated request with no tenant claim.
        var tenantId = _userContext.TenantId ?? throw new IdNotFoundException("Tenant", Guid.Empty);

        var limits = await _limitService.GetAllAsync(tenantId, ct);
        var response = limits.Select(l => new TenantLimitResponse
        {
            ResourceKey = l.ResourceKey,
            MaxCount = l.MaxCount,
            Used = l.Used,
            Source = l.Source.ToString(),
            // The override reason is the SuperAdmin's internal note - not for the tenant's eyes.
            OverrideReason = null,
            IsOverLimit = l.IsOverLimit,
        }).ToList();

        return Result<List<TenantLimitResponse>>.Success(response, _operation);
    }
}
