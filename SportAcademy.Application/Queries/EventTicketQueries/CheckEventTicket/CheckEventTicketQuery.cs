using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.EventTicketQueries.CheckEventTicket
{
    // The door scanner read a ticket: say whether it can go in, without letting it in yet (the
    // doorman decides - AdmitEventTicketCommand). Code is the scanned QR (its link or token);
    // EventId + Number is the fallback when the code can't be scanned.
    public record CheckEventTicketQuery(string? Code, int? EventId, int? Number)
        : IRequest<Result<EventTicketCheckDto>>, IRequiresFeature
    {
        public string FeatureKey => "event-management";
    }
}
