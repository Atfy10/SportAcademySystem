using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.AuthCommands.UpdateUserBranches;

public record UpdateUserBranchesCommand(Guid UserId, List<int> BranchIds) : IRequest<Result<bool>>;
