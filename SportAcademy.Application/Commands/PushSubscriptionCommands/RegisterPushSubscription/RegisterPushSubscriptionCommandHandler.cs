using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PushSubscriptionCommands.RegisterPushSubscription;

public class RegisterPushSubscriptionCommandHandler
    : IRequestHandler<RegisterPushSubscriptionCommand, Result<bool>>
{
    private readonly IPushSubscriptionRepository _repository;
    private readonly IUserContextService _userContext;
    private readonly string _operation = OperationType.Add.ToString();

    public RegisterPushSubscriptionCommandHandler(
        IPushSubscriptionRepository repository, IUserContextService userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    public async Task<Result<bool>> Handle(RegisterPushSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId
            ?? throw new InvalidOperationException("RegisterPushSubscriptionCommand invoked without an authenticated user.");

        await _repository.AddOrUpdateAsync(new PushSubscription
        {
            UserId = userId,
            Endpoint = request.Endpoint,
            P256dh = request.P256dh,
            Auth = request.Auth,
            UserAgent = request.UserAgent,
            CreatedAt = DateTime.UtcNow,
        }, cancellationToken);

        return Result<bool>.Success(true, _operation);
    }
}
