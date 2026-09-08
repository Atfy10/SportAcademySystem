using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.TraineeGroupCommands.PauseTraineeGroup;

public record PauseTraineeGroupCommand(int Id, string? Reason) : IRequest<Result<bool>>, IRequiresFeature
{
    public string FeatureKey => "group-management";
}
