using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.InvitationDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.AuthCommands.ResendInvitation;

public record ResendInvitationCommand(
    Guid TenantId,
    string Email,
    Guid InvitedByUserId) : IRequest<Result<InvitationResponse>>, IRequiresFeature
{
    public string FeatureKey => "user-management";
}
