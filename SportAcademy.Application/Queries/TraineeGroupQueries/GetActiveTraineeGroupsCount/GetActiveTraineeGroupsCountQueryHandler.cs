using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.TraineeGroupQueries.GetActiveTraineeGroupsCount
{
    public class GetActiveTraineeGroupsCountQueryHandler : IRequestHandler<GetActiveTraineeGroupsCountQuery, Result<int>>
    {
        private readonly ITraineeGroupRepository _traineeGroupRepository;
        public GetActiveTraineeGroupsCountQueryHandler(ITraineeGroupRepository traineeGroupRepository)
        {
            _traineeGroupRepository = traineeGroupRepository;
        }
        public async Task<Result<int>> Handle(GetActiveTraineeGroupsCountQuery request, CancellationToken cancellationToken)
        {
            var count = await _traineeGroupRepository.GetActiveTraineeGroupsCountAsync(cancellationToken);
            return Result<int>.Success(count, "Get Active Trainee Groups Count");
        }
    }
}
