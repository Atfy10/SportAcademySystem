using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SportExceptions;

namespace SportAcademy.Application.Commands.SportCommands.AddSkillLevel;

public class AddSkillLevelCommandHandler : IRequestHandler<AddSkillLevelCommand, Result<string>>
{
    private readonly ISportRepository _sportRepository;
    private readonly string _operation = OperationType.Add.ToString();

    public AddSkillLevelCommandHandler(ISportRepository sportRepository)
    {
        _sportRepository = sportRepository;
    }

    public async Task<Result<string>> Handle(AddSkillLevelCommand request, CancellationToken cancellationToken)
    {
        var sport = await _sportRepository.GetByIdAsync(request.SportId, cancellationToken)
            ?? throw new SportNotFoundException(request.SportId.ToString());

        return Result<string>.Success(
            $"Skill level '{request.Name}' added to sport '{sport.Name}'.",
            _operation);
    }
}
