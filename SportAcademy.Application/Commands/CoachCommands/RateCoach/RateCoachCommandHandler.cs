using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Commands.CoachCommands.RateCoach;

public class RateCoachCommandHandler : IRequestHandler<RateCoachCommand, Result<bool>>
{
    private readonly ICoachRepository _coachRepository;
    private readonly string _operationType = OperationType.Update.ToString();

    public RateCoachCommandHandler(ICoachRepository coachRepository)
    {
        _coachRepository = coachRepository;
    }

    public async Task<Result<bool>> Handle(RateCoachCommand request, CancellationToken cancellationToken)
    {
        var coach = await _coachRepository.GetByIdAsync(request.CoachId, cancellationToken)
            ?? throw new IdNotFoundException(nameof(Coach), request.CoachId.ToString());

        coach.Rate = request.Rate;

        await _coachRepository.UpdateAsync(coach, cancellationToken);

        return Result<bool>.Success(true, _operationType);
    }
}
