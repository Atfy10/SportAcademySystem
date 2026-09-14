using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BranchExceptions;
using SportAcademy.Domain.Exceptions.TraineeExceptions;

namespace SportAcademy.Application.Commands.Trainees.UpdateTraineeAcademicInfo
{
    public class UpdateTraineeAcademicInfoCommandHandler : IRequestHandler<UpdateTraineeAcademicInfoCommand, Result<bool>>
    {
        private readonly IBranchRepository _branchRepository;
        private readonly ITraineeRepository _traineeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;
        private readonly string _operationType = OperationType.Update.ToString();

        public UpdateTraineeAcademicInfoCommandHandler(
            IBranchRepository branchRepository,
            ITraineeRepository traineeRepository,
            IUnitOfWork unitOfWork,
            IPublisher publisher)
        {
            _branchRepository = branchRepository;
            _traineeRepository = traineeRepository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<Result<bool>> Handle(UpdateTraineeAcademicInfoCommand request, CancellationToken cancellationToken)
        {
            var trainee = await _traineeRepository.GetFullTrainee(request.Id, cancellationToken)
                ?? throw new TraineeNotFoundException(request.Id.ToString());

            var isBranchExist = await _branchRepository.IsExistAsync(request.BranchId, cancellationToken);
            if (!isBranchExist)
                throw new BranchNotFoundException(request.BranchId.ToString());

            trainee.BranchId = request.BranchId;

            var addedSportIds = await _traineeRepository.UpdateSports(trainee, request.SportIds);

            cancellationToken.ThrowIfCancellationRequested();

            await _traineeRepository.UpdateAsyncWithoutSave(trainee, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            // See CreateTraineeCommandHandler for why this is needed: UpdateSports bypasses
            // SportTrainee's own CreateSportTraineeCommandHandler, so without this, a sport
            // added to a trainee here would never get an initial skill-history row.
            foreach (var sportId in addedSportIds)
            {
                await _publisher.Publish(
                    new SportTraineeSkillLevelChangedEvent(trainee.Id, sportId, SkillLevel.NotSpecified, SkillLevel.NotSpecified),
                    cancellationToken);
            }

            return Result<bool>.Success(true, _operationType);
        }
    }
}
