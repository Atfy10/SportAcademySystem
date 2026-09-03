using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.ExcuseRequestCommands.CreateExcuseRequest;

public record CreateExcuseRequestCommand(
    int SessionOccurrenceId,
    int TraineeId,
    string Reason
) : IRequest<Result<int>>, IRequiresFeature
{
    public string FeatureKey => "attendance-tracking";
}
