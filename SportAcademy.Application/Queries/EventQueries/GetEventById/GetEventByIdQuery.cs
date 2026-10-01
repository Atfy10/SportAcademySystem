using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.EventQueries.GetEventById;

public record GetEventByIdQuery(int Id) : IRequest<Result<EventDetailsDto>>, IRequiresFeature
{
    public string FeatureKey => "event-management";
}
