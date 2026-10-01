using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.EventTicketCommands.UpdateEventTicket
{
    // Names (or un-names, with a blank GuestName) one ticket.
    public record UpdateEventTicketCommand(int Id, string? GuestName) : IRequest<Result<EventTicketDto>>, IRequiresFeature
    {
        public string FeatureKey => "event-management";
    }
}
