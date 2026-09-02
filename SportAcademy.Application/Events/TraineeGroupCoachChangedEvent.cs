using MediatR;

namespace SportAcademy.Application.Events;

public sealed record TraineeGroupCoachChangedEvent(
    int TraineeGroupId, int? OldCoachId, int NewCoachId, int SportId) : INotification;
