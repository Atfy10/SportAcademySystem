using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.AuthCommands.AdminResetUserPassword;

public record AdminResetUserPasswordCommand(
    Guid UserId,
    string AdminPassword,
    string NewPassword
) : IRequest<Result<bool>>;

public record AdminResetUserPasswordRequest(
    string AdminPassword,
    string NewPassword
);