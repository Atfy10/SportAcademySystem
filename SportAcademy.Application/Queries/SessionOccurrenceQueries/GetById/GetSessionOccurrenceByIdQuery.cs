using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;

namespace SportAcademy.Application.Queries.SessionOccurrenceQueries.GetById
{
    public record GetSessionOccurrenceByIdQuery(int Id) : IRequest<Result<SessionOccurrenceDto>>;
}
