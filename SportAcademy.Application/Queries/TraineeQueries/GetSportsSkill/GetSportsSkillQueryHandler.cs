using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Queries.TraineeQueries.GetSkillProgress;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.TraineeExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.TraineeQueries.GetSportsSkill
{
    public class GetSportsSkillQueryHandler : IRequestHandler<GetSportsSkillQuery, Result<List<TraineeSportsSkillDto>>>
    {
        private readonly ITraineeRepository _traineeRepository;
        private readonly ITraineeCareerEventRepository _careerEventRepository;
        private readonly string _operationType = OperationType.Get.ToString();

        public GetSportsSkillQueryHandler(
            ITraineeRepository traineeRepository,
            ITraineeCareerEventRepository careerEventRepository)
        {
            _traineeRepository = traineeRepository;
            _careerEventRepository = careerEventRepository;
        }

        public async Task<Result<List<TraineeSportsSkillDto>>> Handle(GetSportsSkillQuery request, CancellationToken cancellationToken)
        {
            var trainee = await _traineeRepository.GetFullTrainee(request.TraineeId, cancellationToken)
                ?? throw new TraineeNotFoundException(request.TraineeId.ToString());

            var result = new List<TraineeSportsSkillDto>();

            foreach (var sportTrainee in trainee.Sports)
            {
                var sportName = sportTrainee.Sport?.Name ?? string.Empty;

                result.Add(new TraineeSportsSkillDto(
                    sportTrainee.SportId, sportName, sportTrainee.SkillLevel));
            }

            return Result<List<TraineeSportsSkillDto>>.Success(result, _operationType);
        }
    }
}
