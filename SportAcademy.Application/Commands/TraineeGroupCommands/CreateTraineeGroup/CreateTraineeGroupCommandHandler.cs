using AutoMapper;
using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.TraineeGroupCommands.CreateTraineeGroup
{
    public class CreateTraineeGroupCommandHandler : IRequestHandler<CreateTraineeGroupCommand, Result<int>>
    {
        private readonly ITraineeGroupRepository _traineeGroupRepository;
        private readonly IMapper _mapper;
        private readonly string _operationType = OperationType.Add.ToString();

        public CreateTraineeGroupCommandHandler(
            ITraineeGroupRepository traineeGroupRepository,
            IMapper mapper)
        {
            _traineeGroupRepository = traineeGroupRepository;
            _mapper = mapper;
        }

        public async Task<Result<int>> Handle(CreateTraineeGroupCommand request, CancellationToken cancellationToken)
        {
            var traineeGroup = _mapper.Map<TraineeGroup>(request)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            cancellationToken.ThrowIfCancellationRequested();

            await _traineeGroupRepository.AddAsync(traineeGroup, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<int>.Success(traineeGroup.Id, _operationType);
        }
    }
}
