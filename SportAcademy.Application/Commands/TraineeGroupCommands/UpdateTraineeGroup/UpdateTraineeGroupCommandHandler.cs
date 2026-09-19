using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Translations;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Exceptions.TraineeGroupExceptions;

namespace SportAcademy.Application.Commands.TraineeGroupCommands.UpdateTraineeGroup
{
    public class UpdateTraineeGroupCommandHandler : IRequestHandler<UpdateTraineeGroupCommand, Result<TraineeGroupDto>>
    {
        private readonly ITraineeGroupRepository _traineeGroupRepository;
        private readonly ICoachRepository _coachRepository;
        private readonly ISessionOccurrenceRepository _sessionOccurrenceRepository;
        private readonly ITenantClock _tenantClock;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;
        private readonly string _operationType = OperationType.Update.ToString();

        public UpdateTraineeGroupCommandHandler(
            ITraineeGroupRepository traineeGroupRepository,
            ICoachRepository coachRepository,
            ISessionOccurrenceRepository sessionOccurrenceRepository,
            ITenantClock tenantClock,
            IUnitOfWork unitOfWork,
            IPublisher publisher)
        {
            _traineeGroupRepository = traineeGroupRepository;
            _coachRepository = coachRepository;
            _sessionOccurrenceRepository = sessionOccurrenceRepository;
            _tenantClock = tenantClock;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<Result<TraineeGroupDto>> Handle(UpdateTraineeGroupCommand request, CancellationToken cancellationToken)
        {
            var traineeGroup = await _traineeGroupRepository.GetByIdWithTranslationsAsync(request.Id, cancellationToken)
                ?? throw new TraineeGroupNotFoundException($"{request.Id}");

            var newCoach = await _coachRepository.GetByIdAsync(request.CoachId, cancellationToken)
                ?? throw new IdNotFoundException(nameof(Coach), request.CoachId.ToString());
            if (request.SkillLevel > newCoach.SkillLevel)
                throw new CoachSkillLevelTooLowException(request.CoachId, newCoach.SkillLevel, request.SkillLevel);

            // Reassigning to a coach of a different sport would silently change the sport of
            // every trainee already enrolled in this group (TraineeGroup has no independent
            // SportId - it's always whatever the assigned coach teaches).
            var coachChanged = request.CoachId != traineeGroup.CoachId;
            var oldCoachId = traineeGroup.CoachId;
            if (coachChanged)
            {
                var oldCoach = await _coachRepository.GetByIdAsync(traineeGroup.CoachId, cancellationToken);
                if (oldCoach is not null && newCoach.SportId != oldCoach.SportId)
                    throw new CoachSportMismatchException(traineeGroup.CoachId, request.CoachId);
            }

            TraineeGroupMapper.ApplyUpdate(traineeGroup, request);

            // NameAr == null: leave any existing translation untouched.
            // NameAr == "" (after trim): explicit clear -> delete the translation row.
            // NameAr non-empty: upsert with the given name. No English counterpart to pair it
            // with here - the group's Name is server-generated, not part of this command.
            if (request.NameAr is not null)
            {
                var trimmedName = request.NameAr.Trim();
                var existingTranslation = traineeGroup.Translations.FirstOrDefault(t => t.LangCode == "ar");

                if (trimmedName.Length == 0)
                {
                    if (existingTranslation is not null) traineeGroup.Translations.Remove(existingTranslation);
                }
                else if (existingTranslation is not null)
                {
                    existingTranslation.Name = trimmedName;
                }
                else
                {
                    traineeGroup.Translations.Add(new TraineeGroupTranslation { LangCode = "ar", Name = trimmedName });
                }
            }

            // Schedules == null: leave the weekly pattern untouched. Non-null: replace it,
            // diffing by (Day, StartTime) so an unchanged slot keeps its GroupScheduleId - and
            // every SessionOccurrence already generated against it stays valid. Only slots that
            // were actually removed or moved get their still-Scheduled, not-yet-started
            // occurrences cleaned up; nothing that already started or has attendance is touched.
            if (request.Schedules is not null)
            {
                var newSlots = request.Schedules
                    .Select(s => (s.Day, StartTime: TimeOnly.Parse(s.StartTime)))
                    .ToList();

                var existingSlots = traineeGroup.GroupSchedules.ToList();

                var removedSlots = existingSlots
                    .Where(e => !newSlots.Any(n => n.Day == e.Day && n.StartTime == e.StartTime))
                    .ToList();

                var addedSlots = newSlots
                    .Where(n => !existingSlots.Any(e => e.Day == n.Day && e.StartTime == n.StartTime))
                    .Select(n => new GroupSchedule { Day = n.Day, StartTime = n.StartTime })
                    .ToList();

                if (removedSlots.Count > 0)
                {
                    var tenantNow = await _tenantClock.GetLocalNowAsync(cancellationToken);
                    await _sessionOccurrenceRepository.DeleteFutureScheduledOccurrencesAsync(
                        removedSlots.Select(r => r.Id).ToList(), tenantNow, cancellationToken);

                    foreach (var removed in removedSlots)
                        traineeGroup.GroupSchedules.Remove(removed);
                }

                foreach (var added in addedSlots)
                    traineeGroup.GroupSchedules.Add(added);
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _traineeGroupRepository.UpdateAsyncWithoutSave(traineeGroup, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            await _publisher.Publish(new TraineeGroupUpdatedEvent(traineeGroup.Id), cancellationToken);

            if (coachChanged)
            {
                await _publisher.Publish(new TraineeGroupCoachChangedEvent(
                    traineeGroup.Id, oldCoachId, request.CoachId, newCoach.SportId), cancellationToken);
            }

            return Result<TraineeGroupDto>.Success(TraineeGroupMapper.ToDto(traineeGroup), _operationType);
        }
    }
}
