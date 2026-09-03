using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.CoachCommands.UpdateCoachBranches;

public record UpdateCoachBranchesCommand(int CoachId, List<int> BranchIds) : IRequest<Result<bool>>;
