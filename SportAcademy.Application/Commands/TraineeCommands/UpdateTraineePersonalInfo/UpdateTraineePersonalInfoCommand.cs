using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.Trainees.UpdateTraineePersonalInfo
{
    // True partial update: only Id is required, every other field is optional and left
    // untouched when omitted. Deliberately carries nothing branch/sport-related - see
    // UpdateTraineeAcademicInfoCommand for that half. Not IBranchScopedRequest/
    // IOptionallyBranchScopedRequest: this command never reassigns the trainee's branch, and
    // Trainee is IBranchScoped, so the EF global query filter already stops a branch-restricted
    // caller from ever fetching (and therefore editing) a trainee outside their allowed branches.
    public record UpdateTraineePersonalInfoCommand : IRequest<Result<bool>>, IRequiresFeature
    {
        public string FeatureKey => "trainee-management";
        public int Id { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? GuardianName { get; init; }
        public string? ParentNumber { get; init; }
        public List<string>? MedicalConditions { get; init; }
        public string? ImageUrl { get; init; }
    }
}
