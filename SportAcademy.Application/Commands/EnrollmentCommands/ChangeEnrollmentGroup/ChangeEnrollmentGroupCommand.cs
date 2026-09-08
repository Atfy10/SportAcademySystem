using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.EnrollmentCommands.ChangeEnrollmentGroup;

public record ChangeEnrollmentGroupCommand(int EnrollmentId, int NewTraineeGroupId) : IRequest<Result<bool>>, IRequiresFeature
{
    public string FeatureKey => "enrollment-management";
}
