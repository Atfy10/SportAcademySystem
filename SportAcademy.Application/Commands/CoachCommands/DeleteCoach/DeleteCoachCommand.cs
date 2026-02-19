using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.CoachCommands.DeleteCoach
{
    public record DeleteCoachCommand(int EmployeeId) : IRequest<Result<bool>>;
}
