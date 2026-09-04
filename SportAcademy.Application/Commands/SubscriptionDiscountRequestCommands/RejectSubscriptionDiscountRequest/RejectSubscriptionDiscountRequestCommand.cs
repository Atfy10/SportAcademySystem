using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDiscountRequestDtos;

namespace SportAcademy.Application.Commands.SubscriptionDiscountRequestCommands.RejectSubscriptionDiscountRequest
{
    public record RejectSubscriptionDiscountRequestCommand(int Id, string RejectionReason)
        : IRequest<Result<SubscriptionDiscountRequestDto>>;
}
