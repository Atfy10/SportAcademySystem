using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.EventTicketCommands.IssueEventTickets
{
    // Issues Count new numbered tickets for the event (never more than its capacity in total).
    // GuestName names the ticket - only when issuing a single one.
    public record IssueEventTicketsCommand(int EventId, int Count, string? GuestName)
        : IRequest<Result<List<EventTicketDto>>>, IRequiresFeature
    {
        public string FeatureKey => "event-management";
    }
}
