using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.Trainees.UpdateTraineeAcademicInfo
{
    // BranchId/SportIds are always fully overwritten - no partial-update semantics, matching the
    // pre-split UpdateTraineePersonalCommand's behavior for these two fields. The academic edit
    // form always fetches and shows the trainee's current branch/sports before submit, so a
    // full-replace here is unambiguous UX, not a data-loss risk.
    public record UpdateTraineeAcademicInfoCommand(
        int Id,
        int BranchId,
        List<int> SportIds
    ) : IRequest<Result<bool>>, IBranchScopedRequest, IRequiresFeature, IRequiresActiveBranch, IRequiresActiveSports
    {
        public string FeatureKey => "trainee-management";

        // Explicit implementation: SportIds' declared type (List<int>) can't implicitly satisfy
        // IRequiresActiveSports.SportIds (IEnumerable<int>?) - see CreateTraineeCommand's
        // identical note.
        IEnumerable<int>? IRequiresActiveSports.SportIds => SportIds;
    }
}
