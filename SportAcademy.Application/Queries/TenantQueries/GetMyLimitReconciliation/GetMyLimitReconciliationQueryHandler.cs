using System.Text.Json;
using MediatR;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.LimitDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Queries.TenantQueries.GetMyLimitReconciliation;

public class GetMyLimitReconciliationQueryHandler
    : IRequestHandler<GetMyLimitReconciliationQuery, Result<MyLimitReconciliationResponse>>
{
    private readonly IUserContextService _userContext;
    private readonly ITenantRepository _tenantRepository;
    private readonly IEffectiveLimitService _limitService;
    private readonly string _operation = OperationType.Get.ToString();

    public GetMyLimitReconciliationQueryHandler(
        IUserContextService userContext, ITenantRepository tenantRepository, IEffectiveLimitService limitService)
    {
        _userContext = userContext;
        _tenantRepository = tenantRepository;
        _limitService = limitService;
    }

    public async Task<Result<MyLimitReconciliationResponse>> Handle(GetMyLimitReconciliationQuery request, CancellationToken ct)
    {
        // TenantId always has a value here - TenantResolutionMiddleware's post-auth block
        // already 400s any authenticated request with no tenant claim before this handler runs.
        var tenantId = _userContext.TenantId ?? throw new IdNotFoundException("Tenant", Guid.Empty);

        var open = await _tenantRepository.GetOpenReconciliationAsync(tenantId, ct);
        var limits = await _limitService.GetAllAsync(tenantId, ct);

        var response = new MyLimitReconciliationResponse
        {
            Reconciliation = open is null ? null : new LimitReconciliationResponse
            {
                Id = open.Id,
                TenantId = open.TenantId,
                OpenedAt = open.OpenedAt,
                DeadlineAt = open.DeadlineAt,
                IsCompleted = false,
                RequiredResources = JsonSerializer.Deserialize<Dictionary<string, int>>(open.RequiredResourcesJson) ?? [],
            },
            Limits = limits.Select(l => new TenantLimitResponse
            {
                ResourceKey = l.ResourceKey,
                MaxCount = l.MaxCount,
                Used = l.Used,
                Source = l.Source.ToString(),
                OverrideReason = l.OverrideReason,
                IsOverLimit = l.IsOverLimit,
            }).ToList(),
        };

        return Result<MyLimitReconciliationResponse>.Success(response, _operation);
    }
}
