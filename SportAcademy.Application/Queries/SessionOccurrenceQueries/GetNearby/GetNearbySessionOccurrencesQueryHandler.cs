using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SessionOccurrenceExceptions;

namespace SportAcademy.Application.Queries.SessionOccurrenceQueries.GetNearby;

public class GetNearbySessionOccurrencesQueryHandler(
    ISessionOccurrenceRepository sessionOccurrenceRepository)
    : IRequestHandler<GetNearbySessionOccurrencesQuery, Result<List<SessionOccurrenceDto>>>
{
    public async Task<Result<List<SessionOccurrenceDto>>> Handle(
        GetNearbySessionOccurrencesQuery request, CancellationToken cancellationToken)
    {
        var exists = await sessionOccurrenceRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new SessionOccurrenceNotFoundException($"{request.Id}");

        var items = await sessionOccurrenceRepository.GetNearbyOccurrencesAsync(
            request.Id, Math.Max(0, request.Past), Math.Max(0, request.Future), cancellationToken);

        return Result<List<SessionOccurrenceDto>>.Success(items, OperationType.Get.ToString());
    }
}
