using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.EventCustomerQueries.GetEventCustomerByPhone;

// The booking form's "is this a returning customer?" lookup. Succeeds with null data when no
// customer has this number - that's an expected answer, not an error.
public record GetEventCustomerByPhoneQuery(string Phone) : IRequest<Result<EventCustomerDto?>>, IRequiresFeature
{
    public string FeatureKey => "event-management";
}
