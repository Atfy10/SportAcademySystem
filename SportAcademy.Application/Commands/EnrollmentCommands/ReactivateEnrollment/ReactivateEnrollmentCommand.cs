using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EnrollmentDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.EnrollmentCommands.ReactivateEnrollment;

// Resuming a Suspended enrollment isn't a bare toggle: time passed while it was paused, so the
// staff member reactivating it decides how many sessions the trainee still has left (defaulted
// client-side from what's stored) - ExpiryDate is then recomputed server-side from today across
// the group's real training days for that many sessions, the same walk CreateEnrollment does.
public record ReactivateEnrollmentCommand(int Id, int SessionRemaining) : IRequest<Result<EnrollmentDto>>, IRequiresFeature
{
    public string FeatureKey => "enrollment-management";
}
