using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.CoachCommands.UpdateCoach;

public record UpdateCoachCommand : IRequest<Result<bool>>
{
    public int Id { get; init; }
    public int SportId { get; init; }
    public SkillLevel SkillLevel { get; init; }
}
