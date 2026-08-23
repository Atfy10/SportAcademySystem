using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EnrollmentExceptions;
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

            // A trainee may only be enrolled in one group per sport - renewing a subscription
            // carries the existing enrollment forward automatically (see
            // CreateSubscriptionDetailsCommandHandler), and moving groups goes through
            // ChangeEnrollmentGroupCommand, so a second CreateEnrollment for a sport the trainee
            // is already in is always either a duplicate or the wrong tool for the job.
            var sportId = await _traineeGroupRepository.GetSportIdAsync(request.TraineeGroupId, cancellationToken);
            if (sportId is not null)
            {
                var existingEnrollment = await _enrollmentRepository.GetCurrentEnrollmentForSportAsync(
                    request.TraineeId, sportId.Value, cancellationToken);
                if (existingEnrollment is not null)
                    throw new TraineeAlreadyEnrolledInSportException(request.TraineeId, sportId.Value);
            }

            // Set initial values
            var subDetails = await _subRepository.GetSubscriptionDetailsWithSubTypeAsync(
                request.SubscriptionDetailsId, cancellationToken)
                ?? throw new SubscriptionDetailsNotFoundException(request.SubscriptionDetailsId
                .ToString());

            // The subscription being tied to this enrollment must be for the same sport as the
            // group - otherwise renewing that subscription later has no matching enrollment to
            // carry forward (GetCurrentEnrollmentForSportAsync matches by the group's sport),
            // and the group/subscription pairing is nonsensical anyway (nothing else validates
            // this: the subscription and group pickers in the UI are independent dropdowns).
            if (sportId is not null && subDetails.SportId != sportId.Value)
                throw new SubscriptionGroupSportMismatchException(request.SubscriptionDetailsId, request.TraineeGroupId);

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
