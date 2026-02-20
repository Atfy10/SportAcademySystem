using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.TraineeQueries.GetAll
{
    public class GetAllTraineesQueryHandler : IRequestHandler<GetAllTraineesQuery, Result<PagedData<TraineeDto>>>
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

        public async Task<Result<PagedData<TraineeDto>>> Handle(GetAllTraineesQuery request, CancellationToken cancellationToken)
        {
            var traineesDto = await _traineeRepository.GetAllAsync<TraineeDto>(request.Page,
                cancellationToken);

            return Result<PagedData<TraineeDto>>.Success(traineesDto, _operationType);
        }
    }
}
