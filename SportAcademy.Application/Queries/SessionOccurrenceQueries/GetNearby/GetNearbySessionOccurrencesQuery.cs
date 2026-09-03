using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;

namespace SportAcademy.Application.Queries.SessionOccurrenceQueries.GetNearby;

public record GetNearbySessionOccurrencesQuery(int Id, int Past = 3, int Future = 3)
    : IRequest<Result<List<SessionOccurrenceDto>>>;
