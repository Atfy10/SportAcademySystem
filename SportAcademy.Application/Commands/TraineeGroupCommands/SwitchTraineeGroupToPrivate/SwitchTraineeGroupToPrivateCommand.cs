using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.TraineeGroupCommands.SwitchTraineeGroupToPrivate;

// Turns a public group into a private one. Private groups are small, so the caller picks the
// (smaller) capacity and which of the current trainees carry on in it: everyone else's enrollment
// is ended, which frees them to enroll in another group and continue their subscription there.
// Result data = how many enrollments were ended.
public record SwitchTraineeGroupToPrivateCommand(
    int Id,
    int MaximumCapacity,
    IReadOnlyList<int> KeepTraineeIds) : IRequest<Result<int>>, IRequiresFeature
{
    public string FeatureKey => "group-management";
}
