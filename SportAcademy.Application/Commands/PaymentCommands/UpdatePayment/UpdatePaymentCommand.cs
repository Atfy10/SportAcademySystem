using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PaymentCommands.UpdatePayment
{
    // Payment is created automatically alongside SubscriptionDetails (1:1 by schema) - there's
    // no "create a standalone payment" operation. This lets staff correct how an existing
    // payment was recorded (e.g. wrong method, or backdating the paid date) as its own,
    // auditable action instead of only ever being set at subscription-creation time.
    public record UpdatePaymentCommand(
        string PaymentNumber,
        PaymentMethod Method,
        DateTime PaidDate
    ) : IRequest<Result>;
}
