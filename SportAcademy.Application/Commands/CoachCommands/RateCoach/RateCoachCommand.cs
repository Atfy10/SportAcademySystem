using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.CoachCommands.RateCoach;

public record RateCoachCommand(int CoachId, int Rate) : IRequest<Result<bool>>;
