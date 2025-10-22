using MediatR;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.SessionOccurrenceCommands.UpdateSessionOccurrence
{
    public record UpdateSessionOccurrenceCommand(
        int Id,
        DateTime? StartDateTime,
        SessionStatus? Status
    ) : IRequest<Result<SessionOccurrenceDto>>;
}
