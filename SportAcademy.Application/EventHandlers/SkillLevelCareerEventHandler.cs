using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.EventHandlers;

public sealed class SkillLevelCareerEventHandler : INotificationHandler<SportTraineeSkillLevelChangedEvent>
{
    private readonly ITraineeCareerEventRepository _careerEventRepository;

    public SkillLevelCareerEventHandler(ITraineeCareerEventRepository careerEventRepository)
    {
        _careerEventRepository = careerEventRepository;
    }

    public async Task Handle(SportTraineeSkillLevelChangedEvent notification, CancellationToken cancellationToken)
    {
        await _careerEventRepository.AddAsync(new TraineeCareerEvent
        {
            TraineeId = notification.TraineeId,
            EventType = TraineeCareerEventType.SkillLevelChanged,
            SportId = notification.SportId,
            SkillLevel = notification.NewSkillLevel,
            EffectiveDate = DateTime.UtcNow,
        }, cancellationToken);
    }
}
