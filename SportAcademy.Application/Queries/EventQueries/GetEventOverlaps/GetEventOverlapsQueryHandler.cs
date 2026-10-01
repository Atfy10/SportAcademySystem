using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Queries.EventQueries.GetEventOverlaps;

public class GetEventOverlapsQueryHandler : IRequestHandler<GetEventOverlapsQuery, Result<List<EventOverlapDto>>>
{
    private readonly IEventRepository _repository;
    private readonly string _operation = OperationType.GetAll.ToString();

    public GetEventOverlapsQueryHandler(IEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<EventOverlapDto>>> Handle(GetEventOverlapsQuery request, CancellationToken ct)
    {
        if (request.EndsAt <= request.StartsAt)
            return Result<List<EventOverlapDto>>.Success([], _operation);

        // The form sends the times as typed (academy wall clock); the stored columns are UTC.
        var events = await _repository.GetOverlappingAsync(
            request.BranchId, TenantCalendar.ToUtc(request.StartsAt), TenantCalendar.ToUtc(request.EndsAt),
            request.ExcludeId, ct);

        var dtos = events
            .Select(e => new EventOverlapDto(
                e.Id, e.Title, TenantCalendar.ToLocal(e.StartsAt), TenantCalendar.ToLocal(e.EndsAt),
                e.EventCustomer?.FullName ?? string.Empty))
            .ToList();

        return Result<List<EventOverlapDto>>.Success(dtos, _operation);
    }
}
