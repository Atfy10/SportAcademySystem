using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.EventHandlers;

/// <summary>
/// Trigger 2 of the two real-world triggers for a CoachAssigned career event: a trainee becomes
/// associated with a group, whether via a brand-new enrollment or a group transfer - both are
/// "trainee now training under this group's coach" from this listener's point of view, so both
/// CreateEnrollmentCommandHandler and ChangeEnrollmentGroupCommandHandler publish this same event.
/// </summary>
public sealed class EnrollmentCoachSnapshotHandler : INotificationHandler<EnrollmentGroupAssignedEvent>
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ITraineeGroupRepository _traineeGroupRepository;
    private readonly ICoachRepository _coachRepository;
    private readonly ITraineeCareerEventRepository _careerEventRepository;

    public EnrollmentCoachSnapshotHandler(
        IEnrollmentRepository enrollmentRepository,
        ITraineeGroupRepository traineeGroupRepository,
        ICoachRepository coachRepository,
        ITraineeCareerEventRepository careerEventRepository)
    {
        _enrollmentRepository = enrollmentRepository;
        _traineeGroupRepository = traineeGroupRepository;
        _coachRepository = coachRepository;
        _careerEventRepository = careerEventRepository;
    }

    public async Task Handle(EnrollmentGroupAssignedEvent notification, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(notification.EnrollmentId, cancellationToken);
        var group = await _traineeGroupRepository.GetByIdAsync(notification.TraineeGroupId, cancellationToken);
        if (enrollment is null || group is null)
            return;

        var coach = await _coachRepository.GetByIdAsync(group.CoachId, cancellationToken);

        await _careerEventRepository.AddAsync(new TraineeCareerEvent
        {
            TraineeId = enrollment.TraineeId,
            EventType = TraineeCareerEventType.CoachAssigned,
            SportId = coach?.SportId,
            TraineeGroupId = notification.TraineeGroupId,
            CoachId = group.CoachId,
            EnrollmentId = notification.EnrollmentId,
            EffectiveDate = notification.EffectiveDate,
        }, cancellationToken);
    }
}
