using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TenantDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.TenantQueries.GetTenantSettings;

public class GetTenantSettingsQueryHandler : IRequestHandler<GetTenantSettingsQuery, Result<TenantSettingsDto>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserContextService _userContext;
    private readonly string _operation = OperationType.Get.ToString();

    public GetTenantSettingsQueryHandler(ITenantRepository tenantRepository, IUserContextService userContext)
    {
        _tenantRepository = tenantRepository;
        _userContext = userContext;
    }

    public async Task<Result<TenantSettingsDto>> Handle(GetTenantSettingsQuery request, CancellationToken ct)
    {
        var tenantId = _userContext.TenantId;
        if (tenantId is null)
            return Result<TenantSettingsDto>.Failure(_operation, "Tenant ID is not available.", 400);

        var settings = await _tenantRepository.GetSettingsAsync(tenantId.Value, ct);
        if (settings is null)
            return Result<TenantSettingsDto>.Failure(_operation, "Settings not found.", 404);

        var dto = new TenantSettingsDto
        {
            TimeZone = settings.TimeZone,
            Language = settings.Language,
            DateFormat = settings.DateFormat,
            TimeFormat = settings.TimeFormat,
            Currency = settings.Currency
        };

        return Result<TenantSettingsDto>.Success(dto, _operation);
    }
}
