using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDiscountRequestDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.SubscriptionDiscountRequestCommands.ApproveSubscriptionDiscountRequest
{
    public record ApproveSubscriptionDiscountRequestCommand(int Id) : IRequest<Result<SubscriptionDiscountRequestDto>>, IRequiresFeature
    {
        public string FeatureKey => "discount-offers";
    }
}
