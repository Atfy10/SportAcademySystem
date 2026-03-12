using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Queries.EmployeeQueries.SearchEmployeess;
using System.Diagnostics;

namespace SportAcademy.Application.Queries.TraineeQueries.SearchTrainee
{
    public class SearchTraineeQueryHandler
        : IRequestHandler<SearchTraineeQuery, Result<PagedData<TraineeCardDto>>>
    {
        private readonly ITraineeRepository _traineeRepository;

        public SearchTraineeQueryHandler(ITraineeRepository traineeRepository)
        {
            _traineeRepository = traineeRepository;
        }

        public async Task<Result<PagedData<TraineeCardDto>>> Handle(
            SearchTraineeQuery request,
            CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            if (string.IsNullOrWhiteSpace(request.Term))
                return Result<PagedData<TraineeCardDto>>.Failure(nameof(SearchEmployeeQuery), "Search term required");

            if (request.Term.Trim().Length < 2)
                return Result<PagedData<TraineeCardDto>>.Failure(nameof(SearchEmployeeQuery), "Minimum 2 characters");

            var trainees = await _traineeRepository.SearchAsync(
                request.Term,
                request.Page,
                cancellationToken
            );

            sw.Stop();

            return Result<PagedData<TraineeCardDto>>.Success(trainees, nameof(SearchTraineeQuery));
        }
    }
}
