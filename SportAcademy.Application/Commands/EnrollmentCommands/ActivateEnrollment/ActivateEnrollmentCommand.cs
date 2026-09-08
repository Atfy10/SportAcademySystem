using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.EnrollmentCommands.ActivateEnrollment;

public record ActivateEnrollmentCommand(int Id) : IRequest<Result<bool>>, IRequiresFeature
{
    public string FeatureKey => "enrollment-management";
}
