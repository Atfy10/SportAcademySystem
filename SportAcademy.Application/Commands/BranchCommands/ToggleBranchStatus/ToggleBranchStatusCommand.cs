using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.BranchCommands.ToggleBranchStatus;

public record ToggleBranchStatusCommand(int Id) : IRequest<Result<bool>>;
