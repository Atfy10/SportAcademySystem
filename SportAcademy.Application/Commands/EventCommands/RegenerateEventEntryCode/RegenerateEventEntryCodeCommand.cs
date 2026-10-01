using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.EventCommands.RegenerateEventEntryCode
{
    // Replaces the event's entry QR code (e.g. it was shared with the wrong people). The old code
    // stops working at once; people already let in stay counted.
    public record RegenerateEventEntryCodeCommand(int Id) : IRequest<Result<EventDetailsDto>>, IRequiresFeature
    {
        public string FeatureKey => "event-management";
    }
}
