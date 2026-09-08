using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.ExcuseRequestCommands.RejectExcuseRequest;

public record RejectExcuseRequestCommand(int Id, string? Note) : IRequest<Result<bool>>, IRequiresFeature
{
    public string FeatureKey => "attendance-tracking";
}
