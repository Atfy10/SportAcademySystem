using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.EventQueries.GetEventById;

public class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, Result<EventDetailsDto>>
{
    private readonly EventDetailsLoader _detailsLoader;
    private readonly string _operation = OperationType.Get.ToString();

    public GetEventByIdQueryHandler(EventDetailsLoader detailsLoader)
    {
        _detailsLoader = detailsLoader;
    }

    public async Task<Result<EventDetailsDto>> Handle(GetEventByIdQuery request, CancellationToken ct)
        => Result<EventDetailsDto>.Success(await _detailsLoader.LoadAsync(request.Id, ct), _operation);
}
