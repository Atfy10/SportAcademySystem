using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.DTOs.TenantDtos;

public record UpdateTenantSettingsCommand(
    string? TimeZone,
    string? Language,
    string? DateFormat,
    string? TimeFormat,
    string? Currency
) : IRequest<Result>, IRequiresFeature
{
    public string FeatureKey => "tenant-settings";
}
