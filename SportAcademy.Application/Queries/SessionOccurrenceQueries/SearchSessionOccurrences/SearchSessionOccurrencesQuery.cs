using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;

namespace SportAcademy.Application.Queries.SessionOccurrenceQueries.SearchSessionOccurrences;

public record SearchSessionOccurrencesQuery(string Term, PageRequest Page) : IRequest<Result<PagedData<SessionOccurrenceDto>>>;
