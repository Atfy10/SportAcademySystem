using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.AuthCommands.UpdateUserBranches;

public record UpdateUserBranchesCommand(Guid UserId, List<int> BranchIds) : IRequest<Result<bool>>, IRequiresFeature
{
    public string FeatureKey => "user-management";
}
