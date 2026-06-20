using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AuthDtos;

namespace SportAcademy.Application.Commands.AuthCommands.RefreshToken
{
    public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthResponseDto>>;
}
