using MediatR;

namespace SportAcademy.Application.Events;

public sealed record EnrollmentGroupAssignedEvent(
    int EnrollmentId, int TraineeGroupId, DateTime EffectiveDate) : INotification;
