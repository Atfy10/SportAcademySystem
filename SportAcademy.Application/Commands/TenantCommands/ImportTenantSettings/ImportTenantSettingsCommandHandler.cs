using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TenantDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.TenantCommands.ImportTenantSettings;

public class ImportTenantSettingsCommandHandler : IRequestHandler<ImportTenantSettingsCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContextService _userContext;
    private readonly string _operation = OperationType.Update.ToString();

    public ImportTenantSettingsCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork,
        IUserContextService userContext)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result> Handle(ImportTenantSettingsCommand request, CancellationToken ct)
    {
        var tenantId = _userContext.TenantId;
        if (tenantId is null)
            return Result.Failure(_operation, "Tenant ID is not available.", 400);

        if (request.Data.Version != 1)
            return Result.Failure(_operation, "Unsupported export version.", 400);

        var settings = await _tenantRepository.GetSettingsAsync(tenantId.Value, ct);
        if (settings is not null)
        {
            settings.TimeZone = request.Data.Settings.TimeZone;
            settings.Language = request.Data.Settings.Language;
            settings.DateFormat = request.Data.Settings.DateFormat;
            settings.TimeFormat = request.Data.Settings.TimeFormat;
            settings.Currency = request.Data.Settings.Currency;
            _tenantRepository.UpdateSettings(settings);
        }

        var featureUpdates = request.Data.Features
            .ToDictionary(f => f.FeatureId, f => f.IsEnabled);

        await _tenantRepository.BulkUpdateFeaturesAsync(tenantId.Value, featureUpdates, "TenantAdmin", ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(_operation, "Settings imported successfully.");
    }
}
