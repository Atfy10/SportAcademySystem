using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.TraineeGroupQueries.GetAllTraineeGroupsCount
{
    public class GetAllTraineeGroupsCountQueryHandler: IRequestHandler<GetAllTraineeGroupsCountQuery, Result<int>>
    {
        private readonly ITraineeGroupRepository _traineeGroupRepository;
        public GetAllTraineeGroupsCountQueryHandler(ITraineeGroupRepository traineeGroupRepository)
        {
            _traineeGroupRepository = traineeGroupRepository;
        }
        public async Task<Result<int>> Handle(GetAllTraineeGroupsCountQuery request, CancellationToken cancellationToken)
        {
            var count = await _traineeGroupRepository.GetAllTraineeGroupsCountAsync(cancellationToken);
            return Result<int>.Success(count, "Get Trainee Groups Count");
        }
    }
}
