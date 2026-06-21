using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.BranchCommands.RemoveSportFromBranch
{
    public record RemoveSportFromBranchCommand(
        int SportId,
        int BranchId
    ) : IRequest<Result<string>>;
}
