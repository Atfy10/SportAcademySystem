using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeDtos;

namespace SportAcademy.Application.Queries.TraineeQueries.GetSportsSkill
{
    public record GetSportsSkillQuery(int TraineeId) : IRequest<Result<List<TraineeSportsSkillDto>>>;
}
