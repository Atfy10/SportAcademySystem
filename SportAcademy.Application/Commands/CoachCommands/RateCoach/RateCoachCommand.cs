using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.CoachCommands.RateCoach;

public record RateCoachCommand(int CoachId, int Rate) : IRequest<Result<bool>>, IRequiresFeature
{
    public string FeatureKey => "coach-management";
}
