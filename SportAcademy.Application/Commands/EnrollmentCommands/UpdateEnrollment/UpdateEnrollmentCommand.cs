using MediatR;
using SportAcademy.Application.DTOs.EnrollmentDtos;
using SportAcademy.Application.Services;

namespace SportAcademy.Application.Commands.EnrollmentCommands.UpdateEnrollment
{
    public record UpdateEnrollmentCommand(
        int Id,
        DateTime? ExpiryDate,
        int? SessionRemaining,
        bool? IsActive
    ) : IRequest<Result<EnrollmentDto>>;
}
