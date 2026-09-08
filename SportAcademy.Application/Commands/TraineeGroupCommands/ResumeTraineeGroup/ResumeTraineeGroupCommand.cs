using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.TraineeGroupCommands.ResumeTraineeGroup;

public record ResumeTraineeGroupCommand(int Id) : IRequest<Result<bool>>, IRequiresFeature
{
    public string FeatureKey => "group-management";
}
