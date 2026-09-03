using MediatR;
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
    private readonly IPublisher _publisher;
    private readonly string _operation = OperationType.Update.ToString();

    public ToggleUserActiveCommandHandler(
        IUserRepository userRepository,
        IUserContextService userContext,
        IPublisher publisher)
    {
        _userRepository = userRepository;
        _userContext = userContext;
        _publisher = publisher;
    }

    public async Task<Result<bool>> Handle(ToggleUserActiveCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new IdNotFoundException(nameof(AppUser), request.UserId);

        user.IsBanned = !user.IsBanned;
        await _userRepository.UpdateAsync(user, cancellationToken);

        var actorName = _userContext.UserId is { } actorId
            ? await _userRepository.GetDisplayNameAsync(actorId, cancellationToken)
            : "System";
        await _publisher.Publish(new UserActiveStatusChangedEvent(user.Id, user.IsBanned, actorName), cancellationToken);

        return Result<bool>.Success(!user.IsBanned, _operation);
    }
}
