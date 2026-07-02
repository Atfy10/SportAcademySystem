using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AuthDtos;

namespace SportAcademy.Application.Commands.AuthCommands.AcceptInvitation;

public record AcceptInvitationCommand(
    string RawToken,
    string Password,
    string Slug) : IRequest<Result<AuthResponseDto>>;
