using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.EventCommands.CancelEvent
{
    // Mode only matters when money was already collected - see EventCancellationMode.
    public record CancelEventCommand(int Id, string Reason, EventCancellationMode Mode)
        : IRequest<Result<EventDetailsDto>>, IRequiresFeature
    {
        public string FeatureKey => "event-management";
    }
}
