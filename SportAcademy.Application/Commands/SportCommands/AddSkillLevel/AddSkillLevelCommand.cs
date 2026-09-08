using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.SportCommands.AddSkillLevel;

public record AddSkillLevelCommand(int SportId, string Name, string? Description) : IRequest<Result<string>>, IRequiresFeature
{
    public string FeatureKey => "sport-management";
}
