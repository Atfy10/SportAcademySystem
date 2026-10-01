using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.EventQueries.GetEventOverlaps;

// Other live bookings at the same branch whose time overlaps the given span - the booking form
// warns about them (overlaps are allowed: a big venue can host two things at once). ExcludeId
// leaves out the event being edited.
public record GetEventOverlapsQuery(int BranchId, DateTime StartsAt, DateTime EndsAt, int? ExcludeId)
    : IRequest<Result<List<EventOverlapDto>>>, IRequiresFeature
{
    public string FeatureKey => "event-management";
}
