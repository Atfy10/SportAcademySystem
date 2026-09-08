using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.InvitationDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.AuthCommands.CreateInvitation;

// IRequiresFeature only actually applies to the staff-invite path (Role set): the legacy
// "claim a brand-new tenant" invite (Role null) is sent by the Platform SuperAdmin creating the
// tenant, whose JWT carries no tenant claim - FeatureGateBehavior skips gating entirely when
// IUserContextService.TenantId is null, so that bootstrapping flow is unaffected regardless of
// this tenant's user-management setting.
public record CreateInvitationCommand(
    Guid TenantId,
    string Email,
    Guid InvitedByUserId,
    // Null Role = legacy "claim a brand-new tenant" invite (always becomes Owner, tenant
    // must be PendingSetup). A Role = staff invite into an already-Active tenant.
    string? Role = null,
    List<string>? Permissions = null,
    DateTime? ExpiresAt = null,
    // Required (non-empty) when Role == "Employee" - the only branch-restricted role. Ignored
    // for every other role.
    List<int>? BranchIds = null) : IRequest<Result<InvitationResponse>>, IRequiresFeature
{
    public string FeatureKey => "user-management";
}
