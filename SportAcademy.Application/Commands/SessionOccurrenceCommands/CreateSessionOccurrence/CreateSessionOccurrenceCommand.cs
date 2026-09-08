using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.SessionOccurrenceCommands.CreateSessionOccurrence
{
    public record CreateSessionOccurrenceCommand(
        int GroupScheduleId,
        DateTime StartDateTime,
        SessionStatus Status
    ) : IRequest<Result<int>>, IRequiresFeature
    {
        public string FeatureKey => "session-management";
    }
}
