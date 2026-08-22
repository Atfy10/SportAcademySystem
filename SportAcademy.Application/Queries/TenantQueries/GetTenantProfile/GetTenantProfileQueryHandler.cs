using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TenantDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.TenantQueries.GetTenantProfile;

public class GetTenantProfileQueryHandler : IRequestHandler<GetTenantProfileQuery, Result<TenantProfileDto>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserContextService _userContext;
    private readonly string _operation = OperationType.Get.ToString();

    public GetTenantProfileQueryHandler(ITenantRepository tenantRepository, IUserContextService userContext)
    {
        _tenantRepository = tenantRepository;
        _userContext = userContext;
    }

    public async Task<Result<TenantProfileDto>> Handle(GetTenantProfileQuery request, CancellationToken ct)
    {
        var tenantId = _userContext.TenantId;
        if (tenantId is null)
            return Result<TenantProfileDto>.Failure(_operation, "Tenant ID is not available.", 400);

        var profile = await _tenantRepository.GetProfileAsync(tenantId.Value, ct);
        if (profile is null)
            return Result<TenantProfileDto>.Failure(_operation, "Profile not found.", 404);

        var dto = new TenantProfileDto
        {
            OrganizationName = profile.OrganizationName,
            LogoUrl = profile.LogoUrl,
            Email = profile.Email,
            Phone = profile.Phone,
            Website = profile.Website,
            Address = profile.Address,
            TaxNumber = profile.TaxNumber,
            CommercialRegistration = profile.CommercialRegistration,
            Description = profile.Description
        };

        return Result<TenantProfileDto>.Success(dto, _operation);
    }
}
