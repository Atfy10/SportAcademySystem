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
        private readonly ITraineeRepository _traineeRepository;
        private readonly ISportTraineeRepository _sportTraineeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;
        private readonly string _operationType = OperationType.Add.ToString();

        public CreateEnrollmentCommandHandler(
            IEnrollmentRepository enrollmentRepository,
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            ITraineeGroupRepository traineeGroupRepository,
            ITraineeRepository traineeRepository,
            ISportTraineeRepository sportTraineeRepository,
            IUnitOfWork unitOfWork,
            IPublisher publisher)
        {
            _enrollmentRepository = enrollmentRepository;
            _subRepository = subscriptionDetailsRepository;
            _traineeGroupRepository = traineeGroupRepository;
            _traineeRepository = traineeRepository;
            _sportTraineeRepository = sportTraineeRepository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<Result<int>> Handle(CreateEnrollmentCommand request, CancellationToken cancellationToken)
        {
            var enrollment = EnrollmentMapper.ToEntity(request);

            // With schedules: the enrollment's expiry is counted across the group's actual
            // training days further down, not copied from the subscription's own estimate.
            var group = await _traineeGroupRepository.GetByIdWithSchedulesAsync(request.TraineeGroupId, cancellationToken)
                ?? throw new TraineeGroupNotFoundException(request.TraineeGroupId.ToString());

            if (!group.IsActive)
                throw new TraineeGroupInactiveException(group.Id, group.InactiveReason);

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

            // Public and private training are priced separately, so a subscription may only be
            // spent in a group of the type it was priced for.
            if (group.Type != subDetails.GroupType)
                throw new SubscriptionGroupTypeMismatchException(
                    request.SubscriptionDetailsId, request.TraineeGroupId, subDetails.GroupType, group.Type);

            // The expiry is counted across the days this group actually trains on, starting from
            // the day the trainee joins it - the same walk that produced the subscription's own
            // end date, but against the real group rather than the pattern picked at purchase
            // time. Since only groups matching that pattern are offered for assignment, the two
            // normally land on the same date; doing the walk here rather than copying
            // subDetails.EndDate keeps it correct even when the enrollment starts later than the
            // subscription did (a trainee assigned to a group a few days after purchasing).
            var trainingDays = group.GroupSchedules.Select(gs => gs.Day).Distinct().ToList();
            if (trainingDays.Count == 0)
                throw new GroupHasNoScheduleException(group.Id);

            var subscriptionType = subDetails.SportPrice.SportSubscriptionType.SubscriptionType;
            var totalSessions = TrainingScheduleService.CalculateTotalSessions(
                subscriptionType.DaysPerMonth, subscriptionType.NumberOfMonths);

            enrollment.ExpiryDate = TrainingScheduleService
                .ComputeEndDate(DateOnly.FromDateTime(enrollment.EnrollmentDate), totalSessions, trainingDays)
                .ToDateTime(TimeOnly.MinValue);

            // A trainee can only join a group whose gender policy accepts them (Mixed accepts
            // anyone) and whose required skill level is at or below their own for this sport.
            var trainee = await _traineeRepository.GetFullTrainee(request.TraineeId, cancellationToken);
            var genderOk = trainee is null || group.Gender switch
            {
                TraineeGroupGender.Mixed => true,
                TraineeGroupGender.Male => trainee.Gender == Gender.Male,
                TraineeGroupGender.Female => trainee.Gender == Gender.Female,
                _ => true
            };
            if (!genderOk)
                throw new TraineeGenderMismatchException(request.TraineeId, request.TraineeGroupId);

            if (sportId is not null)
            {
                var sportTrainee = await _sportTraineeRepository.GetByIdWithIncludesAsync(
                    sportId.Value, request.TraineeId, cancellationToken);
                // NotSpecified means "no skill on record" (including the SportTrainee row
                // CreateSubscriptionDetailsCommandHandler auto-backfills at subscription time)
                // just as much as sportTrainee being null does - nothing to enforce against
                // absent data either way.
                if (sportTrainee is not null
                    && sportTrainee.SkillLevel != SkillLevel.NotSpecified
                    && group.SkillLevel > sportTrainee.SkillLevel)
                    throw new TraineeSkillLevelTooLowException(request.TraineeId, request.TraineeGroupId);
            }

            var daysPerMonth = SubscriptionDetailsService.CalculateAllowedSessions(subDetails);
            enrollment.SessionAllowed = daysPerMonth;
            enrollment.SessionRemaining = enrollment.SessionAllowed;
            enrollment.IsActive = true;

            cancellationToken.ThrowIfCancellationRequested();

            // A trainee who lapsed past the grace window left this group (EnrollmentLapseService
            // closed the row), but their history with it - attendance, the original join date -
            // is still attached to that enrollment. Coming back to the SAME group reopens it
            // rather than starting a parallel row, which is what "their enrollment becomes
            // active again" means in practice: EndDate cleared, pointed at the new subscription,
            // sessions and expiry recomputed. Joining a DIFFERENT group is a genuinely new
            // enrollment and falls through to the insert below, leaving the closed row as history.
            var reopened = await _enrollmentRepository.GetEndedEnrollmentForGroupAsync(
                request.TraineeId, request.TraineeGroupId, cancellationToken);

            if (reopened is not null)
            {
                reopened.EndDate = null;
                reopened.IsActive = true;
                reopened.SubscriptionDetailsId = request.SubscriptionDetailsId;
                reopened.ExpiryDate = enrollment.ExpiryDate;
                reopened.SessionAllowed = enrollment.SessionAllowed;
                reopened.SessionRemaining = enrollment.SessionAllowed;

                await _enrollmentRepository.UpdateAsync(reopened, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                await _publisher.Publish(new EnrollmentGroupAssignedEvent(
                    reopened.Id, request.TraineeGroupId, reopened.EnrollmentDate), cancellationToken);

                return Result<int>.Success(reopened.Id, _operationType);
            }

            await _enrollmentRepository.AddAsyncWithoutSave(enrollment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            await _publisher.Publish(new EnrollmentCreatedEvent(enrollment.Id), cancellationToken);
            await _publisher.Publish(new EnrollmentGroupAssignedEvent(
                enrollment.Id, request.TraineeGroupId, enrollment.EnrollmentDate), cancellationToken);

            return Result<int>.Success(enrollment.Id, _operationType);
        }
    }
}
