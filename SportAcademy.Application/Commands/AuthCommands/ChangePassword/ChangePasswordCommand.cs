using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.AuthCommands.ChangePassword;

public record ChangePasswordCommand(string CurrentPassword, string NewPassword)
    : IRequest<Result<bool>>, IRequiresFeature
{
    public string FeatureKey => "profile-mgmt";
}
