using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.CoachCommands.UpdateCoach;

public record UpdateCoachCommand : IRequest<Result<bool>>, IRequiresFeature
{
    public int Id { get; init; }
    public int SportId { get; init; }
    public SkillLevel SkillLevel { get; init; }
    public string FeatureKey => "coach-management";
}
