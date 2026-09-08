using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.DTOs.TenantDtos;

// backup-restore, not tenant-settings - this is the "Backup & restore" card in DangerZone.tsx
// (matches the catalog feature's DisplayName exactly), a distinct capability from the
// UpdateTenantSettingsCommand form ("Tenant Configuration") that tenant-settings actually gates.
public record ImportTenantSettingsCommand(ExportTenantSettingsDto Data) : IRequest<Result>, IRequiresFeature
{
    public string FeatureKey => "backup-restore";
}
