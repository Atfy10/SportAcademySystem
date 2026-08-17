using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EnrollmentDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.EnrollmentCommands.UpdateEnrollment
{
    public record UpdateEnrollmentCommand(
        int Id,
        DateTime? ExpiryDate,
        int? SessionRemaining,
        bool? IsActive
    ) : IRequest<Result<EnrollmentDto>>, IRequiresFeature
    {
        public string FeatureKey => "enrollment-management";
    }
}
