using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Queries.SportQueries.SearchSports;

public record SearchSportsQuery(string SearchTerm) : IRequest<Result<IReadOnlyList<string>>>;
