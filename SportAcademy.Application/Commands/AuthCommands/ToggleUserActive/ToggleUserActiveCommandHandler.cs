using MediatR;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Commands.AuthCommands.ToggleUserActive;

public class ToggleUserActiveCommandHandler : IRequestHandler<ToggleUserActiveCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContextService _userContext;
    private readonly IEffectiveLimitService _limitService;
    private readonly IPublisher _publisher;
    private readonly string _operation = OperationType.Update.ToString();

    public ToggleUserActiveCommandHandler(
        IUserRepository userRepository,
        IUserContextService userContext,
        IEffectiveLimitService limitService,
        IPublisher publisher)
    {
        _userRepository = userRepository;
        _userContext = userContext;
        _limitService = limitService;
        _publisher = publisher;
    }

    public async Task<Result<bool>> Handle(ToggleUserActiveCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new IdNotFoundException(nameof(AppUser), request.UserId);

        // Only un-banning (true -> false) consumes a seat - this single command flips both
        // directions, so the check can't live in a pipeline behavior the way create-time
        // gating does (it would also wrongly block banning a user while already at the cap,
        // which must always be allowed since it frees a seat, not consume one). Un-gated, a
        // tenant could simply re-enable a deactivated user to bypass the wizard's whole point.
        var isReactivating = user.IsBanned;
        if (isReactivating && _userContext.TenantId is { } tenantId)
        {
            var limit = await _limitService.GetAsync(tenantId, LimitedResources.Users, cancellationToken);
            if (!limit.HasHeadroom)
                return Result<bool>.Failure(_operation, limit.ToMessage(), 403, limit.ToErrorDictionary());
        }

        user.IsBanned = !user.IsBanned;
        await _userRepository.UpdateAsync(user, cancellationToken);

        var actorName = _userContext.UserId is { } actorId
            ? await _userRepository.GetDisplayNameAsync(actorId, cancellationToken)
            : "System";
        await _publisher.Publish(new UserActiveStatusChangedEvent(user.Id, user.IsBanned, actorName), cancellationToken);

        return Result<bool>.Success(!user.IsBanned, _operation);
    }
}
