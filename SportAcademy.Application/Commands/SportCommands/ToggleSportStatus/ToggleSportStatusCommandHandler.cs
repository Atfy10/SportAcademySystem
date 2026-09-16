using MediatR;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SportExceptions;

namespace SportAcademy.Application.Commands.SportCommands.ToggleSportStatus;

// Mirrors ToggleBranchStatusCommandHandler exactly, including the reactivation-only limit check
// (see that handler's comment - a blind pipeline gate on the whole command would also wrongly
// block deactivating a sport while already at the cap).
public class ToggleSportStatusCommandHandler : IRequestHandler<ToggleSportStatusCommand, Result<bool>>
{
    private readonly ISportRepository _sportRepository;
    private readonly IUserContextService _userContext;
    private readonly IEffectiveLimitService _limitService;
    private readonly string _operation = OperationType.Update.ToString();

    public ToggleSportStatusCommandHandler(
        ISportRepository sportRepository, IUserContextService userContext, IEffectiveLimitService limitService)
    {
        _sportRepository = sportRepository;
        _userContext = userContext;
        _limitService = limitService;
    }

    public async Task<Result<bool>> Handle(ToggleSportStatusCommand request, CancellationToken cancellationToken)
    {
        var sport = await _sportRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new SportNotFoundException(request.Id.ToString());

        if (!sport.IsActive && _userContext.TenantId is { } tenantId)
        {
            var limit = await _limitService.GetAsync(tenantId, LimitedResources.Sports, cancellationToken);
            if (!limit.HasHeadroom)
                return Result<bool>.Failure(_operation, limit.ToMessage(), 403, limit.ToErrorDictionary());
        }

        var newStatus = await _sportRepository.ToggleIsActiveAsync(request.Id, cancellationToken);
        return Result<bool>.Success(newStatus!.Value, _operation);
    }
}
