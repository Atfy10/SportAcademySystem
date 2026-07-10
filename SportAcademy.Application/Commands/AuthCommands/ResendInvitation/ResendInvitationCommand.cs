using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.InvitationDtos;

namespace SportAcademy.Application.Commands.AuthCommands.ResendInvitation;

public record ResendInvitationCommand(
    Guid TenantId,
    string Email,
    Guid InvitedByUserId) : IRequest<Result<InvitationResponse>>;
