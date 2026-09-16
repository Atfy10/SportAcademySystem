using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.SportCommands.ToggleSportStatus;

// Mirrors ToggleBranchStatusCommand exactly - see Sport.IsActive for why this exists.
public record ToggleSportStatusCommand(int Id) : IRequest<Result<bool>>, IRequiresFeature
{
    public string FeatureKey => "sport-management";
}
