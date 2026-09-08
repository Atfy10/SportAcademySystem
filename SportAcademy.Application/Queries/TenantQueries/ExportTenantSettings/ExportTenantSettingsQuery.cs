using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TenantDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.TenantQueries.ExportTenantSettings;

// Gated (unlike most reads elsewhere in this pass) because it's narrowly Backup & Restore's own
// action, not shared/cross-cutting data used elsewhere in the app - see GetTenantSettingsQuery/
// GetTenantProfileQuery's own reasoning for why *those* stay open (logo + date/currency
// formatting used app-wide) despite living on the same Settings page.
public record ExportTenantSettingsQuery : IRequest<Result<ExportTenantSettingsDto>>, IRequiresFeature
{
    public string FeatureKey => "backup-restore";
}
