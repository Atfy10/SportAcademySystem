using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.FinanceCommands.RefundPayment;

public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, Result<bool>>
{
    private readonly IFinanceLedgerService _financeLedgerService;
    private readonly IUserContextService _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IPublisher _publisher;
    private readonly string _operation = OperationType.Update.ToString();

    public RefundPaymentCommandHandler(
        IFinanceLedgerService financeLedgerService,
        IUserContextService userContext,
        IUserRepository userRepository,
        IPublisher publisher)
    {
        _financeLedgerService = financeLedgerService;
        _userContext = userContext;
        _userRepository = userRepository;
        _publisher = publisher;
    }

    public async Task<Result<bool>> Handle(RefundPaymentCommand request, CancellationToken ct)
    {
        await _financeLedgerService.RefundPaymentAsync(request.PaymentNumber, request.Amount, ct);

        var actorName = _userContext.UserId is { } userId
            ? await _userRepository.GetDisplayNameAsync(userId, ct)
            : "System";
        await _publisher.Publish(new PaymentRefundedEvent(request.PaymentNumber, request.Amount, actorName), ct);

        return Result<bool>.Success(true, _operation);
    }
}
