using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.AuthCommands.AssignRolesToUser;

public record AssignRolesToUserCommand(Guid UserId, List<string> Roles) : IRequest<Result<bool>>, IRequiresFeature
{
    public string FeatureKey => "role-management";
}
