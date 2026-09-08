using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.AuthCommands.AdminResetUserPassword;

public record AdminResetUserPasswordCommand(
    Guid UserId,
    string AdminPassword,
    string NewPassword
) : IRequest<Result<bool>>, IRequiresFeature
{
    public string FeatureKey => "user-management";
}

public record AdminResetUserPasswordRequest(
    string AdminPassword,
    string NewPassword
);