using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.EventTicketCommands.AdmitEventTicket
{
    // The doorman tapped "Admit" on a scanned ticket. The ticket is the scanned Code (its QR
    // link or token) or, when it couldn't be scanned, EventId + Number. Every check runs again
    // here - nothing from the earlier scan is trusted.
    public record AdmitEventTicketCommand(string? Code, int? EventId, int? Number)
        : IRequest<Result<EventTicketCheckDto>>, IRequiresFeature
    {
        public string FeatureKey => "event-management";
    }
}
