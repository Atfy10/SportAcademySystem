using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.AuthCommands.RevokeToken
{
    public record RevokeTokenCommand(string RefreshToken) : IRequest<Result<bool>>;
}
