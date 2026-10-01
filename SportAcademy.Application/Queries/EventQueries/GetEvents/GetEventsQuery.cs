using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.EventQueries.GetEvents;

public record GetEventsQuery(
    PageRequest Page,
    int? BranchId,
    DateOnly? From,
    DateOnly? To,
    EventStatus? Status,
    int? CustomerId,
    Guid? CreatedByUserId,
    string? Term
) : IRequest<Result<PagedData<EventDto>>>, IRequiresFeature
{
    public string FeatureKey => "event-management";
}
