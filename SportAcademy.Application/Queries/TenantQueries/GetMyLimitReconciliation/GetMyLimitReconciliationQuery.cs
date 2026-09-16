using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.LimitDtos;

namespace SportAcademy.Application.Queries.TenantQueries.GetMyLimitReconciliation;

// The current tenant's own open reconciliation (or null) plus its full effective-limit picture -
// what the reconciliation wizard loads to render the branch/sport/user pickers and running
// usage. TenantId comes from the caller's own JWT claim, never a request parameter - a tenant
// can only ever see its own reconciliation, mirroring TenantController's self-service pattern
// (as opposed to Platform/TenantsController's cross-tenant access).
public record GetMyLimitReconciliationQuery : IRequest<Result<MyLimitReconciliationResponse>>;

public record MyLimitReconciliationResponse
{
    /// <summary>Null when the tenant has no open reconciliation.</summary>
    public LimitReconciliationResponse? Reconciliation { get; init; }
    public List<TenantLimitResponse> Limits { get; init; } = [];
}
