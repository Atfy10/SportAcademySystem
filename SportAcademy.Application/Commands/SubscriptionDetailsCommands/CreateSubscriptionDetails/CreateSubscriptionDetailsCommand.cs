using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.CreateSubscriptionDetails
{
    // No EndDate: it's computed server-side from the plan's session count walked across
    // TrainingDays, so it can't disagree with the schedule the trainee will actually train on.
    // GroupType (public/private) is chosen here because it's what selects the price.
    //
    // PayDeposit: false (default) = the full total is recorded as paid now. true = only
    // DepositAmount is recorded now; the rest stays owed on the invoice until BalanceDueDate
    // (the collect date), after which it's flagged overdue.
    public record CreateSubscriptionDetailsCommand(
        DateOnly StartDate,
        int TraineeId,
        int SubscriptionTypeId,
        int SportId,
        int BranchId,
        TraineeGroupType GroupType,
        List<DayOfWeek> TrainingDays,
        int PaymentTypeId,
        bool PayDeposit = false,
        decimal? DepositAmount = null,
        DateOnly? BalanceDueDate = null
        ) : IRequest<Result<SubscriptionCreatedDto>>, IBranchScopedRequest, IRequiresFeature, IRequiresActiveBranch, IRequiresActiveSport
    {
        public string FeatureKey => "enrollment-management";
    }
}
