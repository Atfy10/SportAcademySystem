using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Queries.CoachQueries.GetCoachsCount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.TraineeQueries.GetTraineesCount
{
    public class GetTraineesCountQueryHandler : IRequestHandler<GetTraineesCountQuery, Result<int>>
    {
        private readonly ITraineeRepository _traineeRepository;
        public GetTraineesCountQueryHandler(ITraineeRepository traineeRepository)
        {
            _traineeRepository = traineeRepository;
        }
        public async Task<Result<int>> Handle(GetTraineesCountQuery request, CancellationToken cancellationToken)
        {
            var allTrainees = await _traineeRepository.GetAllAsync(cancellationToken);
            if (!allTrainees.Any())
                return Result<int>.Success(0, "Get Trainees Count");
            return Result<int>.Success(allTrainees.Count(), "Get Trainees Count");
        }
    }
}
