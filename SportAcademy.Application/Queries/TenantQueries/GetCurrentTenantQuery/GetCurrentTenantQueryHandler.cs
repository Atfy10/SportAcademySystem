using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TenantDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.TenantQueries.GetCurrentTenantQuery;

public class GetCurrentTenantQueryHandler : IRequestHandler<GetCurrentTenantQuery, Result<CurrentTenantResponse>>
{
    private readonly IBaseRepository<Tenant, Guid> _tenantRepository;
    private readonly ITenantRepository _tenants;
    private readonly IUserContextService _userContext;
    private readonly string _operation = OperationType.Get.ToString();

    public GetCurrentTenantQueryHandler(
        IBaseRepository<Tenant, Guid> tenantRepository,
        ITenantRepository tenants,
        IUserContextService userContext)
    {
        _tenantRepository = tenantRepository;
        _tenants = tenants;
        _userContext = userContext;
    }

    public async Task<Result<CurrentTenantResponse>> Handle(GetCurrentTenantQuery request, CancellationToken ct)
    {
        var tenantId = _userContext.TenantId;
        if (tenantId is null)
            return Result<CurrentTenantResponse>.Failure(_operation, "Tenant ID is not available in the context.", 400);

        var tenant = await _tenantRepository.GetByIdAsync(tenantId.Value, ct);
        if (tenant is null)
            return Result<CurrentTenantResponse>.Failure(_operation, "Tenant not found.", 404);

        var profile = await _tenants.GetProfileAsync(tenantId.Value, ct);
        var response = tenant.ToCurrentResponse() with
        {
            LogoUrl = profile?.LogoUrl,
            OrganizationName = profile?.OrganizationName,
        };

        return Result<CurrentTenantResponse>.Success(response, _operation);
    }
}
