using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.SportCommands.AddSkillLevel;

public record AddSkillLevelCommand(int SportId, string Name, string? Description) : IRequest<Result<string>>;
