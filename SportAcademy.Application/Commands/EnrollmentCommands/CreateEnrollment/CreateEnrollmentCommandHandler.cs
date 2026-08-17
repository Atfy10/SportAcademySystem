using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SubscriptonExceptions;
using SportAcademy.Domain.Exceptions.TraineeGroupExceptions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Commands.EnrollmentCommands.CreateEnrollment
{
    public class CreateEnrollmentCommandHandler : IRequestHandler<CreateEnrollmentCommand, Result<int>>
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ISubscriptionDetailsRepository _subRepository;
        private readonly ITraineeGroupRepository _traineeGroupRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;
        private readonly string _operationType = OperationType.Add.ToString();

        public CreateEnrollmentCommandHandler(
            IEnrollmentRepository enrollmentRepository,
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            ITraineeGroupRepository traineeGroupRepository,
            IUnitOfWork unitOfWork,
            IPublisher publisher)
        {
            _enrollmentRepository = enrollmentRepository;
            _subRepository = subscriptionDetailsRepository;
            _traineeGroupRepository = traineeGroupRepository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<Result<int>> Handle(CreateEnrollmentCommand request, CancellationToken cancellationToken)
        {
            var enrollment = EnrollmentMapper.ToEntity(request);

            var group = await _traineeGroupRepository.GetByIdAsync(request.TraineeGroupId, cancellationToken)
                ?? throw new TraineeGroupNotFoundException(request.TraineeGroupId.ToString());

            var activeCount = await _enrollmentRepository.GetActiveEnrollmentCountForGroupAsync(
                request.TraineeGroupId, cancellationToken);
            if (activeCount >= group.MaximumCapacity)
                throw new GroupAtCapacityException(request.TraineeGroupId, group.MaximumCapacity);

            // Set initial values
            var subDetails = await _subRepository.GetSubscriptionDetailsWithSubTypeAsync(
                request.SubscriptionDetailsId, cancellationToken)
                ?? throw new SubscriptionDetailsNotFoundException(request.SubscriptionDetailsId
                .ToString());

            var daysPerMonth = SubscriptionDetailsService.CalculateAllowedSessions(subDetails);
            enrollment.SessionAllowed = daysPerMonth;
            enrollment.SessionRemaining = enrollment.SessionAllowed;
            enrollment.IsActive = true;

            cancellationToken.ThrowIfCancellationRequested();

            await _enrollmentRepository.AddAsyncWithoutSave(enrollment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            await _publisher.Publish(new EnrollmentCreatedEvent(enrollment.Id), cancellationToken);

            return Result<int>.Success(enrollment.Id, _operationType);
        }
    }
}
