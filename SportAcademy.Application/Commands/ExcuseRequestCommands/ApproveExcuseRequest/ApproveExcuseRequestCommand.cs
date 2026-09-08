using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.ExcuseRequestCommands.ApproveExcuseRequest;

public record ApproveExcuseRequestCommand(int Id) : IRequest<Result<bool>>, IRequiresFeature
{
    public string FeatureKey => "attendance-tracking";
}
