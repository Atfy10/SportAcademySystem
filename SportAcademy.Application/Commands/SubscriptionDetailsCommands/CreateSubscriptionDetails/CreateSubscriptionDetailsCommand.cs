using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.CreateSubscriptionDetails
{
    // No EndDate: it's computed server-side from the plan's session count walked across
    // TrainingDays, so it can't disagree with the schedule the trainee will actually train on.
    // GroupType (public/private) is chosen here because it's what selects the price.
    public record CreateSubscriptionDetailsCommand(
        DateOnly StartDate,
        int TraineeId,
        int SubscriptionTypeId,
        int SportId,
        int BranchId,
        TraineeGroupType GroupType,
        List<DayOfWeek> TrainingDays,
        int PaymentTypeId
        ) : IRequest<Result<int>>, IBranchScopedRequest, IRequiresFeature
    {
        public string FeatureKey => "enrollment-management";
    }
}
