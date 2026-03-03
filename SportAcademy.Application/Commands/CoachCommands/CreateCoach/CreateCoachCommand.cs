using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.CoachCommands.CreateCoach;

public record CreateCoachCommand(
    int EmployeeId,
    int SportId,
    SkillLevel SkillLevel
) : IRequest<Result<int>>;
