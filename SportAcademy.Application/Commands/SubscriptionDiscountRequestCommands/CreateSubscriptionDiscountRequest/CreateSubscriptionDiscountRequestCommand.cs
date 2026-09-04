using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDiscountRequestDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.SubscriptionDiscountRequestCommands.CreateSubscriptionDiscountRequest
{
    public record CreateSubscriptionDiscountRequestCommand(
        int TraineeId,
        int SubscriptionTypeId,
        int SportId,
        int BranchId,
        DateOnly StartDate,
        DateOnly EndDate,
        int PaymentTypeId,
        string DiscountCode
    ) : IRequest<Result<SubscriptionDiscountRequestDto>>, IBranchScopedRequest;
}
