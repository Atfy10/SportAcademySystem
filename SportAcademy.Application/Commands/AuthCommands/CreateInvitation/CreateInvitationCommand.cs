using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.InvitationDtos;

namespace SportAcademy.Application.Commands.AuthCommands.CreateInvitation;

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
    List<int>? BranchIds = null) : IRequest<Result<InvitationResponse>>;
