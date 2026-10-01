using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.EventQueries.GetEventsReport;

// The printable events report (Events page, not /finance/reports). Unpaged: every matching
// event in date order, up to a safety cap (Truncated tells the page to ask for a narrower range).
public record GetEventsReportQuery(
    DateOnly? From,
    DateOnly? To,
    int? BranchId,
    EventStatus? Status,
    int? CustomerId,
    Guid? CreatedByUserId
) : IRequest<Result<EventsReportDto>>, IRequiresFeature
{
    public string FeatureKey => "event-management";
}
