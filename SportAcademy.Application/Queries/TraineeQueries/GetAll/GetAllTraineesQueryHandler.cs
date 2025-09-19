using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.TraineeQueries.GetAll
{
    public class GetAllTraineesQueryHandler : IRequestHandler<GetAllTraineesQuery, Result<List<TraineeDto>>>
    {
        private readonly IMapper _mapper;
        private readonly ITraineeRepository _traineeRepository;
        private readonly string _operationType = OperationType.GetAll.ToString();

        public GetAllTraineesQueryHandler(ITraineeRepository traineeRepository,
            IMapper mapper)
        {
            _mapper = mapper;
            _traineeRepository = traineeRepository;
        }
        public async Task<Result<List<TraineeDto>>> Handle(GetAllTraineesQuery request, CancellationToken cancellationToken)
        {
            var trainees = await _traineeRepository.GetAllAsync(cancellationToken) ?? [];

            var traineesDto = _mapper.Map<List<TraineeDto>>(trainees) ?? [];

            return Result<List<TraineeDto>>.Success(traineesDto, _operationType);
        }
    }
}
