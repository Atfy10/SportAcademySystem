using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.EventTicketQueries.GetCheckInEvents
{
    // The door scanner's header: today's events (at the caller's branches) that haven't ended,
    // with how many guests are in so far.
    public record GetCheckInEventsQuery : IRequest<Result<List<CheckInEventDto>>>, IRequiresFeature
    {
        public string FeatureKey => "event-management";
    }
}
