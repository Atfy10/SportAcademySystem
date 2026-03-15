using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.SportQueries.SearchSports
{
    public class SearchSportsQueryHandler : IRequestHandler<SearchSportsQuery, Result<PagedData<SportDto>>>
    {
        private readonly ISportRepository _sportRepository;

        public SearchSportsQueryHandler(ISportRepository sportRepository)
        {
            _sportRepository = sportRepository;
        }

        public async Task<Result<PagedData<SportDto>>> Handle(SearchSportsQuery request, CancellationToken cancellationToken)
        {
            var pagedSports = await _sportRepository.SearchAsync(
                request.SearchTerm, request.Page, cancellationToken);
            
            return Result<PagedData<SportDto>>.Success(pagedSports, nameof(SearchSportsQuery));
        }
    }
}
