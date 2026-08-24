using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Exceptions.TraineeGroupExceptions;

namespace SportAcademy.Application.Commands.TraineeGroupCommands.UpdateTraineeGroup
{
    public class UpdateTraineeGroupCommandHandler : IRequestHandler<UpdateTraineeGroupCommand, Result<TraineeGroupDto>>
    {
        private readonly ITraineeGroupRepository _traineeGroupRepository;
        private readonly ICoachRepository _coachRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;
        private readonly string _operationType = OperationType.Update.ToString();

        public UpdateTraineeGroupCommandHandler(
            ITraineeGroupRepository traineeGroupRepository,
            ICoachRepository coachRepository,
            IUnitOfWork unitOfWork,
            IPublisher publisher)
        {
            _traineeGroupRepository = traineeGroupRepository;
            _coachRepository = coachRepository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<Result<TraineeGroupDto>> Handle(UpdateTraineeGroupCommand request, CancellationToken cancellationToken)
        {
            var traineeGroup = await _traineeGroupRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new TraineeGroupNotFoundException($"{request.Id}");

            var newCoach = await _coachRepository.GetByIdAsync(request.CoachId, cancellationToken)
                ?? throw new IdNotFoundException(nameof(Coach), request.CoachId.ToString());
            if (request.SkillLevel > newCoach.SkillLevel)
                throw new CoachSkillLevelTooLowException(request.CoachId, newCoach.SkillLevel, request.SkillLevel);

            // Reassigning to a coach of a different sport would silently change the sport of
            // every trainee already enrolled in this group (TraineeGroup has no independent
            // SportId - it's always whatever the assigned coach teaches).
            if (request.CoachId != traineeGroup.CoachId)
            {
                var oldCoach = await _coachRepository.GetByIdAsync(traineeGroup.CoachId, cancellationToken);
                if (oldCoach is not null && newCoach.SportId != oldCoach.SportId)
                    throw new CoachSportMismatchException(traineeGroup.CoachId, request.CoachId);
            }

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
