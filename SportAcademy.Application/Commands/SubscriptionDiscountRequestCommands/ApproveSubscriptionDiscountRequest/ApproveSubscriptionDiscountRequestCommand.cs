using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDiscountRequestDtos;

namespace SportAcademy.Application.Commands.SubscriptionDiscountRequestCommands.ApproveSubscriptionDiscountRequest
{
    public record ApproveSubscriptionDiscountRequestCommand(int Id) : IRequest<Result<SubscriptionDiscountRequestDto>>;
}
