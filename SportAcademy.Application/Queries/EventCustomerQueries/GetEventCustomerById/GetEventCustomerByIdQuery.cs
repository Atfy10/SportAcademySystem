using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.EventCustomerQueries.GetEventCustomerById;

public record GetEventCustomerByIdQuery(int Id) : IRequest<Result<EventCustomerDto>>, IRequiresFeature
{
    public string FeatureKey => "event-management";
}
