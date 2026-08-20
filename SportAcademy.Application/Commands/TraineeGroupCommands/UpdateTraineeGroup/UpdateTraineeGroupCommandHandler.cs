using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.TraineeGroupExceptions;

namespace SportAcademy.Application.Commands.TraineeGroupCommands.UpdateTraineeGroup
{
    public class UpdateTraineeGroupCommandHandler : IRequestHandler<UpdateTraineeGroupCommand, Result<TraineeGroupDto>>
    {
        private readonly ITraineeGroupRepository _traineeGroupRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;
        private readonly string _operationType = OperationType.Update.ToString();

        public UpdateTraineeGroupCommandHandler(
            ITraineeGroupRepository traineeGroupRepository,
            IUnitOfWork unitOfWork,
            IPublisher publisher)
        {
            _traineeGroupRepository = traineeGroupRepository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<Result<TraineeGroupDto>> Handle(UpdateTraineeGroupCommand request, CancellationToken cancellationToken)
        {
            var traineeGroup = await _traineeGroupRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new TraineeGroupNotFoundException($"{request.Id}");

            TraineeGroupMapper.ApplyUpdate(traineeGroup, request);

            cancellationToken.ThrowIfCancellationRequested();

            await _traineeGroupRepository.UpdateAsyncWithoutSave(traineeGroup, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            await _publisher.Publish(new TraineeGroupUpdatedEvent(traineeGroup.Id), cancellationToken);

            return Result<TraineeGroupDto>.Success(TraineeGroupMapper.ToDto(traineeGroup), _operationType);
        }
    }
}
