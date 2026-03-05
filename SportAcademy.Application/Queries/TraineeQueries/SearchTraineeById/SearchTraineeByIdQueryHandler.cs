using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.TraineeQueries.SearchTraineeById
{
    public class SearchTraineeByIdQueryHandler : IRequestHandler<SearchTraineeByIdQuery, Result<PagedData<TraineeCardDto>>>
    {
        private readonly ITraineeRepository _traineeRepository;

        public SearchTraineeByIdQueryHandler(ITraineeRepository traineeRepository)
        {
            _traineeRepository = traineeRepository;
        }

        public async Task<Result<PagedData<TraineeCardDto>>> Handle(SearchTraineeByIdQuery request, CancellationToken cancellationToken)
        {
            var isValidId = int.TryParse(request.Term, out var id);
            if (!isValidId) 
                throw new Exception("Invalid ID format. Please provide a valid integer ID.");

            var trainees = await _traineeRepository.SearchByIdAsync(id, request.Page, cancellationToken);

            return Result<PagedData<TraineeCardDto>>.Success(trainees, nameof(SearchTraineeByIdQuery));
        }
    }
}
