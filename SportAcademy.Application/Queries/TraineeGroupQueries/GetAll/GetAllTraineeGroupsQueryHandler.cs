using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.TraineeGroupQueries.GetAll
{
    public class GetAllTraineeGroupsQueryHandler : IRequestHandler<GetAllTraineeGroupsQuery, Result<List<TraineeGroupDto>>>
    {
        private readonly ITraineeGroupRepository _traineeGroupRepository;
        private readonly IMapper _mapper;
        private readonly string _operationType = OperationType.GetAll.ToString();

        public GetAllTraineeGroupsQueryHandler(
            ITraineeGroupRepository traineeGroupRepository,
            IMapper mapper)
        {
            _traineeGroupRepository = traineeGroupRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<TraineeGroupDto>>> Handle(GetAllTraineeGroupsQuery request, CancellationToken cancellationToken)
        {
            var traineeGroups = await _traineeGroupRepository.GetAllAsync(cancellationToken) ?? [];

            var traineeGroupsDto = _mapper.Map<List<TraineeGroupDto>>(traineeGroups) ?? [];

            return Result<List<TraineeGroupDto>>.Success(traineeGroupsDto, _operationType);
        }
    }
}
