using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TenantDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.TenantCommands.UpdateTenantSettings;

public class UpdateTenantSettingsCommandHandler : IRequestHandler<UpdateTenantSettingsCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContextService _userContext;
    private readonly string _operation = OperationType.Update.ToString();

    public UpdateTenantSettingsCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork,
        IUserContextService userContext)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result> Handle(UpdateTenantSettingsCommand request, CancellationToken ct)
    {
        var tenantId = _userContext.TenantId;
        if (tenantId is null)
            return Result.Failure(_operation, "Tenant ID is not available.", 400);

        var settings = await _tenantRepository.GetSettingsAsync(tenantId.Value, ct);
        if (settings is null)
            return Result.Failure(_operation, "Settings not found.", 404);

        if (request.TimeZone is not null) settings.TimeZone = request.TimeZone;
        if (request.Language is not null) settings.Language = request.Language;
        if (request.DateFormat is not null) settings.DateFormat = request.DateFormat;
        if (request.TimeFormat is not null) settings.TimeFormat = request.TimeFormat;
        if (request.Currency is not null) settings.Currency = request.Currency;

        _tenantRepository.UpdateSettings(settings);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(_operation, "Settings updated successfully.");
    }
}
