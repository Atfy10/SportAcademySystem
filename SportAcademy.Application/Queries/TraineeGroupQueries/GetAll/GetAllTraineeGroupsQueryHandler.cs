using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.TraineeGroupQueries.GetAll
{
    public class GetAllTraineeGroupsQueryHandler : IRequestHandler<GetAllTraineeGroupsQuery, Result<PagedData<TraineeGroupDto>>>
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

        public async Task<Result<PagedData<TraineeGroupDto>>> Handle(GetAllTraineeGroupsQuery request, CancellationToken cancellationToken)
        {
            var traineeGroups = await _traineeGroupRepository
                .GetAllPaginatedAsync<TraineeGroup>(request.Page, cancellationToken);

            var traineeGroupsDto = _mapper.Map<PagedData<TraineeGroupDto>>(traineeGroups);

            return Result<PagedData<TraineeGroupDto>>.Success(traineeGroupsDto, _operationType);
        }
    }
}
