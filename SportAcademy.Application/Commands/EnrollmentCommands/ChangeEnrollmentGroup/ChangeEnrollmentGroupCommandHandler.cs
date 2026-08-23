using MediatR;
using SportAcademy.Application.Common.Result;
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

        public ChangeEnrollmentGroupCommandHandler(
            IEnrollmentRepository enrollmentRepository,
            ITraineeGroupRepository traineeGroupRepository)
        {
            _enrollmentRepository = enrollmentRepository;
            _traineeGroupRepository = traineeGroupRepository;
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

            var activeCount = await _enrollmentRepository.GetActiveEnrollmentCountForGroupAsync(
                newGroup.Id, cancellationToken);
            if (activeCount >= newGroup.MaximumCapacity)
                throw new GroupAtCapacityException(newGroup.Id, newGroup.MaximumCapacity);

            enrollment.TraineeGroupId = newGroup.Id;
            await _enrollmentRepository.UpdateAsync(enrollment, cancellationToken);

            return Result<bool>.Success(true, OperationType.Update.ToString());
        }
    }
}
