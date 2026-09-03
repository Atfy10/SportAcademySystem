using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.TraineeGroupCommands.ResumeTraineeGroup;

public record ResumeTraineeGroupCommand(int Id) : IRequest<Result<bool>>;
