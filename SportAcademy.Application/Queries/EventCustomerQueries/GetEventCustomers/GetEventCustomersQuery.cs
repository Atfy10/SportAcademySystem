using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.EventCustomerQueries.GetEventCustomers;

public record GetEventCustomersQuery(
    PageRequest Page, string? Term, int? NationalityCategoryId, bool? IsActive
) : IRequest<Result<PagedData<EventCustomerDto>>>, IRequiresFeature
{
    public string FeatureKey => "event-management";
}
