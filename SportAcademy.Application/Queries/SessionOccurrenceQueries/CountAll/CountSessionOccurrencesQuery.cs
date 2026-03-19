using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.SessionOccurrenceQueries.CountAll;

public record CountSessionOccurrencesQuery : IRequest<Result<int>>;
