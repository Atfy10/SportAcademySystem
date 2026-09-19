using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PushSubscriptionCommands.UnregisterPushSubscription;

public class UnregisterPushSubscriptionCommandHandler
    : IRequestHandler<UnregisterPushSubscriptionCommand, Result<bool>>
{
    private readonly IPushSubscriptionRepository _repository;
    private readonly IUserContextService _userContext;
    private readonly string _operation = OperationType.Delete.ToString();

    public UnregisterPushSubscriptionCommandHandler(
        IPushSubscriptionRepository repository, IUserContextService userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    public async Task<Result<bool>> Handle(UnregisterPushSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId
            ?? throw new InvalidOperationException("UnregisterPushSubscriptionCommand invoked without an authenticated user.");

        await _repository.RemoveByEndpointAsync(userId, request.Endpoint, cancellationToken);

        return Result<bool>.Success(true, _operation);
    }
}
