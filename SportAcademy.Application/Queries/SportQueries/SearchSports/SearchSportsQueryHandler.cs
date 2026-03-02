using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.SportQueries.SearchSports;

public class SearchSportsQueryHandler : IRequestHandler<SearchSportsQuery, Result<IReadOnlyList<string>>>
{
    private readonly ISportRepository _sportRepository;

    public SearchSportsQueryHandler(ISportRepository sportRepository)
    {
        _sportRepository = sportRepository;   
    }

    public async Task<Result<IReadOnlyList<string>>> Handle(SearchSportsQuery request, CancellationToken ct)
    {
        var sports = await _sportRepository.SearchAsync(request.SearchTerm, ct)
            ?? [];

        return Result<IReadOnlyList<string>>.Success(sports, nameof(SearchSportsQuery));
    }
}
