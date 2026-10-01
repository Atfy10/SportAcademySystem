using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.EventTicketQueries.GetEventTickets
{
    // The event page's tickets card: counts plus one page of tickets. Term is a ticket number
    // or part of a guest name.
    public record GetEventTicketsQuery(int EventId, EventTicketFilter Filter, string? Term, PageRequest Page)
        : IRequest<Result<EventTicketsDto>>, IRequiresFeature
    {
        public string FeatureKey => "event-management";
    }
}
