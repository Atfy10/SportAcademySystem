using MediatR;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;
using SportAcademy.Application.Services;
    
namespace SportAcademy.Application.Queries.SessionOccurrenceQueries.GetById
{
    public record GetSessionOccurrenceByIdQuery(int Id) : IRequest<Result<SessionOccurrenceDto>>;
}
