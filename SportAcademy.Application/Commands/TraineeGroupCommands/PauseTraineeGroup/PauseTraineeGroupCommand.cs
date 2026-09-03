using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.TraineeGroupCommands.PauseTraineeGroup;

public record PauseTraineeGroupCommand(int Id, string? Reason) : IRequest<Result<bool>>;
