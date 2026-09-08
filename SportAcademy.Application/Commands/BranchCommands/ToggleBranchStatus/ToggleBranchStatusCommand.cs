using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.BranchCommands.ToggleBranchStatus;

public record ToggleBranchStatusCommand(int Id) : IRequest<Result<bool>>, IRequiresFeature
{
    public string FeatureKey => "branch-management";
}
