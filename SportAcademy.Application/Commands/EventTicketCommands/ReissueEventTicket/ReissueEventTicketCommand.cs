using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.EventTicketCommands.ReissueEventTicket
{
    // Gives an unused ticket a new code and a new number (e.g. it was sent to the wrong person).
    // Same name; the old code and number stop working at once.
    public record ReissueEventTicketCommand(int Id) : IRequest<Result<EventTicketDto>>, IRequiresFeature
    {
        public string FeatureKey => "event-management";
    }
}
