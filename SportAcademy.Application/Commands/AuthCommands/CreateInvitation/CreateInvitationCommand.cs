using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.InvitationDtos;

namespace SportAcademy.Application.Commands.AuthCommands.CreateInvitation;

public record CreateInvitationCommand(
    Guid TenantId,
    string Email,
    Guid InvitedByUserId,
    DateTime? ExpiresAt = null) : IRequest<Result<InvitationResponse>>;
