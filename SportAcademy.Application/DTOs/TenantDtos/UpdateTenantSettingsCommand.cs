using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.DTOs.TenantDtos;

// Country is deliberately absent - it's set once at tenant creation (CreateTenantCommand)
// and permanently locked from then on, since it drives every phone/National ID format rule
// across the academy (CountryRegionalRegistry). There is no update path for it by design.
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
