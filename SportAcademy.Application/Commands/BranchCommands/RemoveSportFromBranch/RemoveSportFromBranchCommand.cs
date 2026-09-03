using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.BranchCommands.RemoveSportFromBranch
{
    public record RemoveSportFromBranchCommand(
        int SportId,
        int BranchId
    ) : IRequest<Result<string>>, IBranchScopedRequest;
}
