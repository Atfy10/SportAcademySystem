using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDiscountRequestDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.SubscriptionDiscountRequestCommands.CreateSubscriptionDiscountRequest
{
    public record CreateSubscriptionDiscountRequestCommand(
        int TraineeId,
        int SubscriptionTypeId,
        int SportId,
        int BranchId,
        DateOnly StartDate,
        TraineeGroupType GroupType,
        List<DayOfWeek> TrainingDays,
        int PaymentTypeId,
        string DiscountCode,
        bool PayDeposit = false,
        decimal? DepositAmount = null,
        DateOnly? BalanceDueDate = null
    ) : IRequest<Result<SubscriptionDiscountRequestDto>>, IBranchScopedRequest, IRequiresFeature, IRequiresActiveBranch, IRequiresActiveSport
    {
        public string FeatureKey => "discount-offers";
    }
}
