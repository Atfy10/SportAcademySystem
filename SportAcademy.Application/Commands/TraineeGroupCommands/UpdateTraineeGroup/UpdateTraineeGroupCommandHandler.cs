using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.TraineeGroupExceptions;

namespace SportAcademy.Application.Commands.TraineeGroupCommands.UpdateTraineeGroup
{
    public class UpdateTraineeGroupCommandHandler : IRequestHandler<UpdateTraineeGroupCommand, Result<TraineeGroupDto>>
    {
        private readonly ITraineeGroupRepository _traineeGroupRepository;
        private readonly string _operationType = OperationType.Update.ToString();

        public UpdateTraineeGroupCommandHandler(ITraineeGroupRepository traineeGroupRepository)
        {
            _traineeGroupRepository = traineeGroupRepository;
        }

        public async Task<Result<TraineeGroupDto>> Handle(UpdateTraineeGroupCommand request, CancellationToken cancellationToken)
        {
            var traineeGroup = await _traineeGroupRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new TraineeGroupNotFoundException($"{request.Id}");

            traineeGroup.ApplyUpdate(request);

            cancellationToken.ThrowIfCancellationRequested();

            await _traineeGroupRepository.UpdateAsync(traineeGroup, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var traineeGroupDto = traineeGroup.ToDto();

            return Result<TraineeGroupDto>.Success(traineeGroupDto, _operationType);
        }
    }
}
