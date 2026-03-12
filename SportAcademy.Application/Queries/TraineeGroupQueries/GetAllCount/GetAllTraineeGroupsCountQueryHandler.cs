using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.TraineeGroupQueries.GetAllCount
{
    public class GetAllTraineeGroupsCountQueryHandler : IRequestHandler<GetAllTraineeGroupsCountQuery, Result<int>>
    {
        private readonly ITraineeGroupRepository _traineeGroupRepository;

        public GetAllTraineeGroupsCountQueryHandler(ITraineeGroupRepository traineeGroupRepository)
        {
            _traineeGroupRepository = traineeGroupRepository;
        }

        public async Task<Result<int>> Handle(GetAllTraineeGroupsCountQuery request, CancellationToken cancellationToken)
        {
            var count = await _traineeGroupRepository.GetCountAsync(cancellationToken);
            
            return Result<int>.Success(count, "GetAllCount");
        }
    }
}
