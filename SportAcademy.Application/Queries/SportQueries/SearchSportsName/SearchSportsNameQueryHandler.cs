using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.SportQueries.SearchSportsName;

public class SearchSportsNameQueryHandler : IRequestHandler<SearchSportsNameQuery, Result<IReadOnlyList<SportDropDownListDto>>>
{
    private readonly ISportRepository _sportRepository;

    public SearchSportsNameQueryHandler(ISportRepository sportRepository)
    {
        _sportRepository = sportRepository;   
    }

    public async Task<Result<IReadOnlyList<SportDropDownListDto>>> Handle(SearchSportsNameQuery request, CancellationToken ct)
    {
        var sports = await _sportRepository.SearchNameAsync(request.SearchTerm, ct)
            ?? [];

        return Result<IReadOnlyList<SportDropDownListDto>>.Success(sports, nameof(SearchSportsNameQuery));
    }
}
