using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TenantDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.TenantQueries.ExportTenantSettings;

public class ExportTenantSettingsQueryHandler : IRequestHandler<ExportTenantSettingsQuery, Result<ExportTenantSettingsDto>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserContextService _userContext;
    private readonly string _operation = OperationType.Get.ToString();

    public ExportTenantSettingsQueryHandler(ITenantRepository tenantRepository, IUserContextService userContext)
    {
        _tenantRepository = tenantRepository;
        _userContext = userContext;
    }

    public async Task<Result<ExportTenantSettingsDto>> Handle(ExportTenantSettingsQuery request, CancellationToken ct)
    {
        var tenantId = _userContext.TenantId;
        if (tenantId is null)
            return Result<ExportTenantSettingsDto>.Failure(_operation, "Tenant ID is not available.", 400);

        var settings = await _tenantRepository.GetSettingsAsync(tenantId.Value, ct);
        var tenantFeatures = await _tenantRepository.GetTenantFeaturesAsync(tenantId.Value, ct);

        var settingsDto = settings is not null ? new TenantSettingsDto
        {
            TimeZone = settings.TimeZone,
            Language = settings.Language,
            DateFormat = settings.DateFormat,
            TimeFormat = settings.TimeFormat,
            Currency = settings.Currency
        } : new TenantSettingsDto
        {
            TimeZone = "Asia/Kuwait",
            Language = "ar-KW",
            DateFormat = "dd/MM/yyyy",
            TimeFormat = "HH:mm",
            Currency = "KWD"
        };

        var featuresDto = tenantFeatures.Select(tf => new ExportTenantFeatureDto
        {
            FeatureId = tf.FeatureId,
            Name = tf.Feature.Name,
            IsEnabled = tf.IsEnabled,
            EnabledAt = tf.EnabledAt
        }).ToList();

        var result = new ExportTenantSettingsDto
        {
            Version = 1,
            ExportedAt = DateTime.UtcNow,
            Settings = settingsDto,
            Features = featuresDto
        };

        return Result<ExportTenantSettingsDto>.Success(result, _operation);
    }
}
