using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.CoachCommands.CreateCoach;

public record CreateCoachCommand(
    int EmployeeId,
    int SportId,
    SkillLevel SkillLevel
) : IRequest<Result<int>>, IRequiresFeature
{
    public string FeatureKey => "coach-management";
}
