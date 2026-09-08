using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.SessionOccurrenceCommands.UpdateSessionOccurrence
{
    public record UpdateSessionOccurrenceCommand(
        int Id,
        DateTime? StartDateTime,
        SessionStatus? Status
    ) : IRequest<Result<SessionOccurrenceDto>>, IRequiresFeature
    {
        public string FeatureKey => "session-management";
    }
}
