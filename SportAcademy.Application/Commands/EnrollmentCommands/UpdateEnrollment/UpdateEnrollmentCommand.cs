using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EnrollmentDtos;

namespace SportAcademy.Application.Commands.EnrollmentCommands.UpdateEnrollment
{
    public record UpdateEnrollmentCommand(
        int Id,
        DateTime? ExpiryDate,
        int? SessionRemaining,
        bool? IsActive
    ) : IRequest<Result<EnrollmentDto>>;
}
