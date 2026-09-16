using System.Text.Json;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.LimitDtos;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.PlatformQueries.GetOpenReconciliations;

public class GetOpenReconciliationsQueryHandler : IRequestHandler<GetOpenReconciliationsQuery, Result<List<LimitReconciliationResponse>>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetOpenReconciliationsQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<List<LimitReconciliationResponse>>> Handle(GetOpenReconciliationsQuery request, CancellationToken ct)
    {
        var open = await _tenantRepository.GetAllOpenReconciliationsAsync(ct);

        var response = open.Select(r => new LimitReconciliationResponse
        {
            Id = r.Id,
            TenantId = r.TenantId,
            TenantDisplayName = r.Tenant?.DisplayName,
            OpenedAt = r.OpenedAt,
            DeadlineAt = r.DeadlineAt,
            IsCompleted = r.CompletedAt is not null,
            RequiredResources = JsonSerializer.Deserialize<Dictionary<string, int>>(r.RequiredResourcesJson) ?? [],
        }).ToList();

        return Result<List<LimitReconciliationResponse>>.Success(response, _operation);
    }
}
