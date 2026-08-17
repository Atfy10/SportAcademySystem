using SportAcademy.Application.Commands.EnrollmentCommands.CreateEnrollment;
using SportAcademy.Application.Commands.EnrollmentCommands.UpdateEnrollment;
using SportAcademy.Application.DTOs.EnrollmentDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.Manual
{
    // Hand-written replacement for the Enrollment <-> EnrollmentDto and
    // CreateEnrollmentCommand/UpdateEnrollmentCommand -> Enrollment mappings in
    // EnrollmentProfile.cs - used only by Create/UpdateEnrollmentCommandHandler.
    public static class EnrollmentMapper
    {
        public static Enrollment ToEntity(CreateEnrollmentCommand cmd) => new()
        {
            EnrollmentDate = cmd.EnrollmentDate,
            ExpiryDate = cmd.ExpiryDate,
            TraineeId = cmd.TraineeId,
            TraineeGroupId = cmd.TraineeGroupId,
            SubscriptionDetailsId = cmd.SubscriptionDetailsId,
        };

        // Partial update: only overwrites fields the command actually carries.
        public static void ApplyUpdate(Enrollment enrollment, UpdateEnrollmentCommand cmd)
        {
            if (cmd.ExpiryDate.HasValue) enrollment.ExpiryDate = cmd.ExpiryDate.Value;
            if (cmd.SessionRemaining.HasValue) enrollment.SessionRemaining = cmd.SessionRemaining.Value;
            if (cmd.IsActive.HasValue) enrollment.IsActive = cmd.IsActive.Value;
        }

        public static EnrollmentDto ToDto(Enrollment enrollment) => new(
            enrollment.Id,
            enrollment.EnrollmentDate,
            enrollment.ExpiryDate,
            enrollment.SessionAllowed,
            enrollment.SessionRemaining,
            enrollment.IsActive,
            enrollment.TraineeId,
            enrollment.TraineeGroupId,
            enrollment.SubscriptionDetailsId
        );
    }
}
