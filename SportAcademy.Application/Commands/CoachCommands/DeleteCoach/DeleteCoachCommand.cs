using MediatR;
using SportAcademy.Application.Services;

namespace SportAcademy.Application.Commands.CoachCommands.DeleteCoach
{
    public record DeleteCoachCommand(int EmployeeId) : IRequest<Result<bool>>;
}
