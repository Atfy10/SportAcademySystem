using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.AuthCommands.ToggleUserActive;

public record ToggleUserActiveCommand(Guid UserId) : IRequest<Result<bool>>, IRequiresFeature
{
    public string FeatureKey => "user-management";
}
