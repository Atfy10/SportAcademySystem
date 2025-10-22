using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.TraineeGroupQueries.GetById
{
    public class GetTraineeGroupByIdQueryHandler : IRequestHandler<GetTraineeGroupByIdQuery, Result<TraineeGroupDto>>
    {
        private readonly ITraineeGroupRepository _traineeGroupRepository;
        private readonly IMapper _mapper;
        private readonly string _operationType = OperationType.Get.ToString();

        public GetTraineeGroupByIdQueryHandler(
            ITraineeGroupRepository traineeGroupRepository,
            IMapper mapper)
        {
            _traineeGroupRepository = traineeGroupRepository;
            _mapper = mapper;
        }

        public async Task<Result<TraineeGroupDto>> Handle(GetTraineeGroupByIdQuery request, CancellationToken cancellationToken)
        {
            var traineeGroup = await _traineeGroupRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new TraineeGroupNotFoundException($"{request.Id}");

            var traineeGroupDto = _mapper.Map<TraineeGroupDto>(traineeGroup)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            return Result<TraineeGroupDto>.Success(traineeGroupDto, _operationType);
        }
    }
}
