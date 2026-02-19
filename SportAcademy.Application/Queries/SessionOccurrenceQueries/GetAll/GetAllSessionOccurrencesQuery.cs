using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;

namespace SportAcademy.Application.Queries.SessionOccurrenceQueries.GetAll
{
    public record GetAllSessionOccurrencesQuery() : IRequest<Result<List<SessionOccurrenceDto>>>;
}
