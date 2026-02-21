using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.TraineeQueries.GetAllTraineesOfSpecificDay
{
    public class GetAllTraineesOfSpecificDayQueryHandler : IRequestHandler<GetAllTraineesOfSpecificDayQuery, Result<PagedData<TraineeOfSpecificDayDto>>>
    {
        private readonly ITraineeRepository _traineeRepository;
        private readonly IMapper _mapper;

        public GetAllTraineesOfSpecificDayQueryHandler(
            ITraineeRepository traineeRepository,
            IMapper mapper)
        {
            _mapper = mapper;
            _traineeRepository = traineeRepository;
        }

        public async Task<Result<PagedData<TraineeOfSpecificDayDto>>> Handle(GetAllTraineesOfSpecificDayQuery request, CancellationToken ct)
        {
            var trainees = await _traineeRepository.GetAllTraineesOfSpecificDayAsync(request.Date, request.Page, ct);

            return Result<PagedData<TraineeOfSpecificDayDto>>.Success(trainees, nameof(GetAllTraineesOfSpecificDayQuery));
        }
    }
}
