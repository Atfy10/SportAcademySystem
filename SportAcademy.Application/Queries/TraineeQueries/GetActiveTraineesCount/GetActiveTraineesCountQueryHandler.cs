using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.TraineeQueries.GetActiveTraineesCount
{
    public class GetActiveTraineesCountQueryHandler : IRequestHandler<GetActiveTraineesCountQuery, Result<int>>
    {
        private readonly ITraineeRepository _traineeRepository;

        public GetActiveTraineesCountQueryHandler(ITraineeRepository traineeRepository)
        {
            _traineeRepository = traineeRepository;
        }

        public async Task<Result<int>> Handle(GetActiveTraineesCountQuery request, CancellationToken cancellationToken)
        {
            var traineesCount = await _traineeRepository.GetActiveTraineesCount(cancellationToken);

            return Result<int>.Success(traineesCount, nameof(GetActiveTraineesCountQuery));
        }
    }
}
