using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.TraineeExceptions;

namespace SportAcademy.Application.Queries.TraineeQueries.GetSkillProgress
{
    public class GetTraineeSkillProgressQueryHandler : IRequestHandler<GetTraineeSkillProgressQuery, Result<List<TraineeSportSkillProgressDto>>>
    {
        private readonly ITraineeRepository _traineeRepository;
        private readonly ITraineeCareerEventRepository _careerEventRepository;
        private readonly string _operationType = OperationType.Get.ToString();

        public GetTraineeSkillProgressQueryHandler(
            ITraineeRepository traineeRepository,
            ITraineeCareerEventRepository careerEventRepository)
        {
            _traineeRepository = traineeRepository;
            _careerEventRepository = careerEventRepository;
        }

        public async Task<Result<List<TraineeSportSkillProgressDto>>> Handle(GetTraineeSkillProgressQuery request, CancellationToken cancellationToken)
        {
            var trainee = await _traineeRepository.GetFullTrainee(request.TraineeId, cancellationToken)
                ?? throw new TraineeNotFoundException(request.TraineeId.ToString());

            var events = await _careerEventRepository.GetSkillEventsForTraineeAsync(request.TraineeId, cancellationToken);

            var result = new List<TraineeSportSkillProgressDto>();

            foreach (var sportTrainee in trainee.Sports)
            {
                var sportEvents = events
                    .Where(e => e.SportId == sportTrainee.SportId)
                    .OrderBy(e => e.EffectiveDate)
                    .ToList();

                if (sportEvents.Count == 0)
                    continue;

                var sportName = sportEvents[0].Sport?.Name ?? string.Empty;

                var history = new List<SkillLevelPeriodDto>();
                for (var i = 0; i < sportEvents.Count; i++)
                {
                    var current = sportEvents[i];
                    var endDate = i + 1 < sportEvents.Count ? sportEvents[i + 1].EffectiveDate : (DateTime?)null;
                    var durationDays = (int)((endDate ?? DateTime.UtcNow) - current.EffectiveDate).TotalDays;

                    history.Add(new SkillLevelPeriodDto(
                        current.SkillLevel ?? sportTrainee.SkillLevel, current.EffectiveDate, endDate, durationDays));
                }

                result.Add(new TraineeSportSkillProgressDto(
                    sportTrainee.SportId, sportName, sportTrainee.SkillLevel, history));
            }

            return Result<List<TraineeSportSkillProgressDto>>.Success(result, _operationType);
        }
    }
}
