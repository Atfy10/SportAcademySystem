using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeDtos;

namespace SportAcademy.Application.Queries.TraineeQueries.GetSkillProgress
{
    public record GetTraineeSkillProgressQuery(int TraineeId) : IRequest<Result<List<TraineeSportSkillProgressDto>>>;
}
