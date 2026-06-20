using SportAcademy.Application.Commands.EnrollmentCommands.CreateEnrollment;
using SportAcademy.Application.DTOs.EnrollmentDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings
{
    public static class EnrollmentMappings
    {
        public static Enrollment ToEnrollment(this CreateEnrollmentCommand cmd, int sessionAllowed)
        {
            return Enrollment.Create(
                cmd.EnrollmentDate,
                cmd.ExpiryDate,
                sessionAllowed,
                cmd.TraineeId,
                cmd.TraineeGroupId,
                cmd.SubscriptionDetailsId);
        }

        public static EnrollmentDto ToDto(this Enrollment enrollment)
        {
            return new EnrollmentDto(
                enrollment.Id,
                enrollment.EnrollmentDate,
                enrollment.ExpiryDate,
                enrollment.SessionAllowed,
                enrollment.SessionRemaining,
                enrollment.IsActive,
                enrollment.TraineeId,
                enrollment.TraineeGroupId,
                enrollment.SubscriptionDetailsId);
        }
    }
}
