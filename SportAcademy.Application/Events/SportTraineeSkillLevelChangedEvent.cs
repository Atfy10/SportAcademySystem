using MediatR;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Events;

public sealed record SportTraineeSkillLevelChangedEvent(
    int TraineeId, int SportId, SkillLevel OldSkillLevel, SkillLevel NewSkillLevel) : INotification;
