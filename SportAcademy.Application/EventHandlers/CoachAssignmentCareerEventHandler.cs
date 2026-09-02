using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.EventHandlers;

/// <summary>
/// Trigger 1 of the two real-world triggers for a CoachAssigned career event: the group's
/// coach itself changes, fanning out to every trainee currently active in that group.
/// </summary>
public sealed class CoachAssignmentCareerEventHandler : INotificationHandler<TraineeGroupCoachChangedEvent>
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ITraineeCareerEventRepository _careerEventRepository;

    public CoachAssignmentCareerEventHandler(
        IEnrollmentRepository enrollmentRepository,
        ITraineeCareerEventRepository careerEventRepository)
    {
        _enrollmentRepository = enrollmentRepository;
        _careerEventRepository = careerEventRepository;
    }

    public async Task Handle(TraineeGroupCoachChangedEvent notification, CancellationToken cancellationToken)
    {
        var activeEnrollments = await _enrollmentRepository.GetActiveEnrollmentsForGroupAsync(
            notification.TraineeGroupId, cancellationToken);

        foreach (var enrollment in activeEnrollments)
        {
            await _careerEventRepository.AddAsyncWithoutSave(new TraineeCareerEvent
            {
                TraineeId = enrollment.TraineeId,
                EventType = TraineeCareerEventType.CoachAssigned,
                SportId = notification.SportId,
                TraineeGroupId = notification.TraineeGroupId,
                CoachId = notification.NewCoachId,
                EnrollmentId = enrollment.Id,
                EffectiveDate = DateTime.UtcNow,
            }, cancellationToken);
        }

        if (activeEnrollments.Count > 0)
            await _careerEventRepository.SaveChangesAsync(cancellationToken);
    }
}
