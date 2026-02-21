using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Queries.TraineeQueries.GetAllTraineesOfSpecificDay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.TraineeQueries.GetTraineesCountOfSpecificDay
{
    public class GetTraineesCountOfSpecificDayQueryHandler : IRequestHandler<GetTraineesCountOfSpecificDayQuery, Result<int>>
    {
        private readonly ITraineeRepository _traineeRepository;

        public GetTraineesCountOfSpecificDayQueryHandler(ITraineeRepository traineeRepository)
        {
            _traineeRepository = traineeRepository;
        }

        public async Task<Result<int>> Handle(GetTraineesCountOfSpecificDayQuery request, CancellationToken cancellationToken)
        {
            var traineesCount = await _traineeRepository.GetTraineesCountOfSpecificDayAsync(request.Date, cancellationToken);

            return Result<int>.Success(traineesCount, nameof(GetTraineesCountOfSpecificDayQuery));
        }
    }
}
