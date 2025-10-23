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

namespace SportAcademy.Application.Commands.TraineeGroupCommands.UpdateTraineeGroup
{
    public class UpdateTraineeGroupCommandHandler : IRequestHandler<UpdateTraineeGroupCommand, Result<TraineeGroupDto>>
    {
        private readonly IMapper _mapper;
        private readonly ITraineeGroupRepository _traineeGroupRepository;
        private readonly string _operationType = OperationType.Update.ToString();

        public UpdateTraineeGroupCommandHandler(
            IMapper mapper,
            ITraineeGroupRepository traineeGroupRepository)
        {
            _mapper = mapper;
            _traineeGroupRepository = traineeGroupRepository;
        }

        public async Task<Result<TraineeGroupDto>> Handle(UpdateTraineeGroupCommand request, CancellationToken cancellationToken)
        {
            var traineeGroup = await _traineeGroupRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new TraineeGroupNotFoundException($"{request.Id}");

            _mapper.Map(request, traineeGroup);

            cancellationToken.ThrowIfCancellationRequested();

            await _traineeGroupRepository.UpdateAsync(traineeGroup, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var traineeGroupDto = _mapper.Map<TraineeGroupDto>(traineeGroup)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            return Result<TraineeGroupDto>.Success(traineeGroupDto, _operationType);
        }
    }
}
