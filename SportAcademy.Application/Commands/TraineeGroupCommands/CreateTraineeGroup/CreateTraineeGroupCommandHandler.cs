using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Exceptions.TraineeGroupExceptions;

namespace SportAcademy.Application.Commands.TraineeGroupCommands.CreateTraineeGroup
{
    public class CreateTraineeGroupCommandHandler : IRequestHandler<CreateTraineeGroupCommand, Result<int>>
    {
        private readonly TraineeGroupService _traineeGroupService;
        private readonly ITraineeGroupRepository _traineeGroupRepository;
        private readonly ICoachRepository _coachRepository;
        private readonly IMapper _mapper;
        private readonly string _operationType = OperationType.Add.ToString();

        public CreateTraineeGroupCommandHandler(
            TraineeGroupService traineeGroupService,
            ITraineeGroupRepository traineeGroupRepository,
            ICoachRepository coachRepository,
            IMapper mapper)
        {
            _traineeGroupService = traineeGroupService;
            _traineeGroupRepository = traineeGroupRepository;
            _coachRepository = coachRepository;
            _mapper = mapper;
        }

        public async Task<Result<int>> Handle(CreateTraineeGroupCommand request, CancellationToken cancellationToken)
        {
            var traineeGroup = _mapper.Map<TraineeGroup>(request)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            // A coach can only lead a group at or below their own skill level. Sport isn't
            // checked here - whichever coach is picked *defines* the group's sport (TraineeGroup
            // has no independent SportId), so there's nothing to mismatch at creation time.
            var coach = await _coachRepository.GetByIdAsync(request.CoachId, cancellationToken)
                ?? throw new IdNotFoundException(nameof(Coach), request.CoachId.ToString());
            if (request.SkillLevel > coach.SkillLevel)
                throw new CoachSkillLevelTooLowException(request.CoachId, coach.SkillLevel, request.SkillLevel);

            cancellationToken.ThrowIfCancellationRequested();

            var tgName = await _traineeGroupService.GenerateTraineeGroupNameAsync(request);
            traineeGroup.Name = tgName;

            // Schedule must be decided at creation time - GroupSchedule rows are added to the
            // TraineeGroup's navigation collection here so EF cascade-inserts them in the same
            // AddAsync/SaveChanges call as the group itself.
            traineeGroup.GroupSchedules = request.Schedules
                .Select(s => new GroupSchedule
                {
                    Day = s.Day,
                    StartTime = TimeOnly.Parse(s.StartTime)
                })
                .ToList();

            await _traineeGroupRepository.AddAsync(traineeGroup, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<int>.Success(traineeGroup.Id, _operationType);
        }
    }
}
