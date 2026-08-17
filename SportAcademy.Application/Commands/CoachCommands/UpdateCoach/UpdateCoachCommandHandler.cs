using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Commands.CoachCommands.UpdateCoach;

public class UpdateCoachCommandHandler : IRequestHandler<UpdateCoachCommand, Result<bool>>
{
    private readonly ICoachRepository _coachRepository;
    private readonly string _operationType = OperationType.Update.ToString();

    public UpdateCoachCommandHandler(ICoachRepository coachRepository)
    {
        _coachRepository = coachRepository;
    }

    public async Task<Result<bool>> Handle(UpdateCoachCommand request, CancellationToken cancellationToken)
    {
        var coach = await _coachRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new IdNotFoundException(nameof(Coach), request.Id.ToString());

        coach.SportId = request.SportId;
        coach.SkillLevel = request.SkillLevel;

        await _coachRepository.UpdateAsync(coach, cancellationToken);

        return Result<bool>.Success(true, _operationType);
    }
}
