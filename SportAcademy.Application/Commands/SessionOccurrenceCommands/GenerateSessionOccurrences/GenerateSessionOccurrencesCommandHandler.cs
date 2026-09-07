using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Exceptions.SessionOccurrenceExceptions;

namespace SportAcademy.Application.Commands.SessionOccurrenceCommands.GenerateSessionOccurrences;

public class GenerateSessionOccurrencesCommandHandler : IRequestHandler<GenerateSessionOccurrencesCommand, Result<int>>
{
    private readonly ITraineeGroupRepository _traineeGroupRepository;
    private readonly ISessionOccurrenceRepository _sessionOccurrenceRepository;
    private readonly IMapper _mapper;
    private readonly ITenantClock _tenantClock;
    private readonly string _operationType = OperationType.Add.ToString();
    private const int MaxDurationDays = 90;

    public GenerateSessionOccurrencesCommandHandler(
        ITraineeGroupRepository traineeGroupRepository,
        ISessionOccurrenceRepository sessionOccurrenceRepository,
        IMapper mapper,
        ITenantClock tenantClock)
    {
        _traineeGroupRepository = traineeGroupRepository;
        _sessionOccurrenceRepository = sessionOccurrenceRepository;
        _mapper = mapper;
        _tenantClock = tenantClock;
    }

    public async Task<Result<int>> Handle(GenerateSessionOccurrencesCommand request, CancellationToken cancellationToken)
    {
        if (request.TraineeGroupId <= 0)
            throw new IdNotFoundException("TraineeGroup", request.TraineeGroupId.ToString());

        if (request.DurationInDays < 1 || request.DurationInDays > MaxDurationDays)
            throw new InvalidDurationException(1, MaxDurationDays);

        var traineeGroup = await _traineeGroupRepository.GetByIdWithSchedulesAsync(request.TraineeGroupId, cancellationToken)
            ?? throw new IdNotFoundException("TraineeGroup", request.TraineeGroupId.ToString());

        var schedules = traineeGroup.GroupSchedules.AsEnumerable();
        if (request.GroupScheduleId.HasValue)
            schedules = schedules.Where(s => s.Id == request.GroupScheduleId.Value);

        var scheduleList = schedules.ToList();
        if (scheduleList.Count == 0)
            throw new NoSchedulesFoundException();

        var lastDateTime = await _sessionOccurrenceRepository.GetLastOccurrenceDateAsync(request.TraineeGroupId, cancellationToken);

        // The tenant's local calendar day, not the server/UTC one - a session generated late in
        // the UTC day for a tenant east of UTC (e.g. Kuwait, UTC+3) must still land on the
        // tenant's own "today", not tomorrow.
        var tenantToday = DateOnly.FromDateTime(await _tenantClock.GetLocalNowAsync(cancellationToken));

        DateOnly startDate;
        if (lastDateTime is null)
        {
            startDate = tenantToday;
        }
        else
        {
            var lastDate = DateOnly.FromDateTime(lastDateTime.Value);
            var daysSinceLast = tenantToday.DayNumber - lastDate.DayNumber;

            if (daysSinceLast > 7)
            {
                if (!request.StartDate.HasValue)
                    throw new SessionGapTooLargeException(lastDate);

                startDate = request.StartDate.Value;
            }
            else
            {
                startDate = lastDate;
            }
        }

        var endDate = startDate.AddDays(request.DurationInDays - 1);
        var sessionsToCreate = new List<SessionOccurrence>();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            foreach (var schedule in scheduleList)
            {
                if (schedule.Day == date.DayOfWeek)
                {
                    var startDateTime = date.ToDateTime(schedule.StartTime);
                    sessionsToCreate.Add(new SessionOccurrence
                    {
                        GroupScheduleId = schedule.Id,
                        StartDateTime = startDateTime,
                        Status = SessionStatus.Scheduled
                    });
                }
            }
        }

        if (sessionsToCreate.Count != 0)
        {
            await _sessionOccurrenceRepository.AddRangeAsync(sessionsToCreate, cancellationToken);
        }

        return Result<int>.Success(sessionsToCreate.Count, _operationType);
    }
}
