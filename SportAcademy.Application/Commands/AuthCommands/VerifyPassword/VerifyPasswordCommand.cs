using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.AuthCommands.VerifyPassword;

public record VerifyPasswordCommand(string Password) : IRequest<Result<bool>>;
