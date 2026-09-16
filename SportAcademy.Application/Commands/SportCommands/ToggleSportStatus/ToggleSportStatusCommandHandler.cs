using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SportExceptions;

namespace SportAcademy.Application.Commands.SportCommands.ToggleSportStatus;

// Mirrors ToggleBranchStatusCommandHandler exactly.
public class ToggleSportStatusCommandHandler : IRequestHandler<ToggleSportStatusCommand, Result<bool>>
{
    private readonly ISportRepository _sportRepository;
    private readonly string _operation = OperationType.Update.ToString();

    public ToggleSportStatusCommandHandler(ISportRepository sportRepository)
    {
        _sportRepository = sportRepository;
    }

    public async Task<Result<bool>> Handle(ToggleSportStatusCommand request, CancellationToken cancellationToken)
    {
        var newStatus = await _sportRepository.ToggleIsActiveAsync(request.Id, cancellationToken);
        if (newStatus is null)
            throw new SportNotFoundException(request.Id.ToString());

        return Result<bool>.Success(newStatus.Value, _operation);
    }
}
