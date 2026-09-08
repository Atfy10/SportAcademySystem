using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.CoachCommands.DeleteCoach
{
    public record DeleteCoachCommand(int EmployeeId) : IRequest<Result<bool>>, IRequiresFeature
    {
        public string FeatureKey => "coach-management";
    }
}
