using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.CoachCommands.UpdateCoachBranches;

public record CoachBranchAccessInput(int BranchId, decimal? Salary);

public record UpdateCoachBranchesCommand(int CoachId, List<CoachBranchAccessInput> Branches) : IRequest<Result<bool>>, IRequiresFeature
{
    public string FeatureKey => "coach-management";
}
