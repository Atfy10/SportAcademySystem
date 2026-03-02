using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.CoachDtos;
using SportAcademy.Application.Interfaces;
using System.Diagnostics;

namespace SportAcademy.Application.Queries.CoachQueries.SearchCoachs
{
    public class SearchCoachQueryHandler : IRequestHandler<SearchCoachQuery, Result<PagedData<CoachCardDto>>>
    {
        private readonly ICoachRepository _coachRepository;
        private readonly IMapper _mapper;

        public SearchCoachQueryHandler(
            ICoachRepository coachRepository,
            IMapper mapper
        )
        {
            _coachRepository = coachRepository;
            _mapper = mapper;
        }

        public async Task<Result<PagedData<CoachCardDto>>> Handle(SearchCoachQuery request, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            if (string.IsNullOrWhiteSpace(request.Term))
                return Result<PagedData<CoachCardDto>>.Failure(nameof(SearchCoachQuery), "Search term required");

            if (request.Term.Trim().Length < 2)
                return Result<PagedData<CoachCardDto>>.Failure(nameof(SearchCoachQuery), "Minimum 2 characters");

            var coaches = await _coachRepository.SearchAsync(request.SearchTerm, request.Page, cancellationToken);

            sw.Stop();

            return Result<PagedData<CoachCardDto>>.Success(coaches, nameof(SearchCoachQuery), $"Search took {sw.ElapsedMilliseconds} ms");
        }
    }
}