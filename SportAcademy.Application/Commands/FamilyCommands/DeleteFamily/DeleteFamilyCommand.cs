using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.FamilyCommands.DeleteFamily;

public record DeleteFamilyCommand(int Id) : IRequest<Result<bool>>, IRequiresFeature
{
    public string FeatureKey => "family-management";
}
