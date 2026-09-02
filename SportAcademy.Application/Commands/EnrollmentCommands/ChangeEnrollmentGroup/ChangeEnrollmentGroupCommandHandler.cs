using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EnrollmentExceptions;
using SportAcademy.Domain.Exceptions.TraineeGroupExceptions;

namespace SportAcademy.Application.Commands.EnrollmentCommands.ChangeEnrollmentGroup
{
    public class ChangeEnrollmentGroupCommandHandler : IRequestHandler<ChangeEnrollmentGroupCommand, Result<bool>>
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ITraineeGroupRepository _traineeGroupRepository;
        private readonly ITraineeRepository _traineeRepository;
        private readonly ISportTraineeRepository _sportTraineeRepository;
        private readonly IPublisher _publisher;

        public ChangeEnrollmentGroupCommandHandler(
            IEnrollmentRepository enrollmentRepository,
            ITraineeGroupRepository traineeGroupRepository,
            ITraineeRepository traineeRepository,
            ISportTraineeRepository sportTraineeRepository,
            IPublisher publisher)
        {
            _enrollmentRepository = enrollmentRepository;
            _traineeGroupRepository = traineeGroupRepository;
            _traineeRepository = traineeRepository;
            _sportTraineeRepository = sportTraineeRepository;
            _publisher = publisher;
        }

        public async Task<Result<bool>> Handle(ChangeEnrollmentGroupCommand request, CancellationToken cancellationToken)
        {
            var enrollment = await _enrollmentRepository.GetByIdAsync(request.EnrollmentId, cancellationToken)
                ?? throw new EnrollmentNotFoundException(request.EnrollmentId.ToString());

            if (enrollment.TraineeGroupId == request.NewTraineeGroupId)
                return Result<bool>.Success(true, OperationType.Update.ToString());

            var newGroup = await _traineeGroupRepository.GetByIdAsync(request.NewTraineeGroupId, cancellationToken)
                ?? throw new TraineeGroupNotFoundException(request.NewTraineeGroupId.ToString());

            var currentSportId = await _traineeGroupRepository.GetSportIdAsync(enrollment.TraineeGroupId, cancellationToken);
            var newSportId = await _traineeGroupRepository.GetSportIdAsync(newGroup.Id, cancellationToken);
            if (currentSportId != newSportId)
                throw new EnrollmentGroupSportMismatchException(request.EnrollmentId, request.NewTraineeGroupId);

            var trainee = await _traineeRepository.GetFullTrainee(enrollment.TraineeId, cancellationToken);
            var genderOk = trainee is null || newGroup.Gender switch
            {
                TraineeGroupGender.Mixed => true,
                TraineeGroupGender.Male => trainee.Gender == Gender.Male,
                TraineeGroupGender.Female => trainee.Gender == Gender.Female,
                _ => true
            };
            if (!genderOk)
                throw new TraineeGenderMismatchException(enrollment.TraineeId, request.NewTraineeGroupId);

            if (newSportId is not null)
            {
                var sportTrainee = await _sportTraineeRepository.GetByIdWithIncludesAsync(
                    newSportId.Value, enrollment.TraineeId, cancellationToken);
                if (sportTrainee is not null && newGroup.SkillLevel > sportTrainee.SkillLevel)
                    throw new TraineeSkillLevelTooLowException(enrollment.TraineeId, request.NewTraineeGroupId);
            }

            var activeCount = await _enrollmentRepository.GetActiveEnrollmentCountForGroupAsync(
                newGroup.Id, cancellationToken);
            if (activeCount >= newGroup.MaximumCapacity)
                throw new GroupAtCapacityException(newGroup.Id, newGroup.MaximumCapacity);

            enrollment.TraineeGroupId = newGroup.Id;
            await _enrollmentRepository.UpdateAsync(enrollment, cancellationToken);

            await _publisher.Publish(new EnrollmentGroupAssignedEvent(
                enrollment.Id, newGroup.Id, DateTime.UtcNow), cancellationToken);

            return Result<bool>.Success(true, OperationType.Update.ToString());
        }
    }
}
