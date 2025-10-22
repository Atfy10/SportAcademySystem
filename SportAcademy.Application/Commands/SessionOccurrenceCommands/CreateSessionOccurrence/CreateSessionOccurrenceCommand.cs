using MediatR;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.SessionOccurrenceCommands.CreateSessionOccurrence
{
    public record CreateSessionOccurrenceCommand(
        int GroupScheduleId,
        DateTime StartDateTime,
        SessionStatus Status
    ) : IRequest<Result<int>>;
}
