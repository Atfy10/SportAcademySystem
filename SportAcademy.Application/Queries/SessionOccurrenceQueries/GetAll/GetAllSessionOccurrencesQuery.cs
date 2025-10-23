using MediatR;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;
using SportAcademy.Application.Services;

namespace SportAcademy.Application.Queries.SessionOccurrenceQueries.GetAll
{
    public record GetAllSessionOccurrencesQuery() : IRequest<Result<List<SessionOccurrenceDto>>>;
}
