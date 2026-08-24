using MediatR;
using SportAcademy.Application.Common.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.CreateSubscriptionDetails
{
    public record CreateSubscriptionDetailsCommand(
        DateOnly StartDate,
        DateOnly EndDate,
        int TraineeId,
        int SubscriptionTypeId,
        int SportId,
        int BranchId,
        int PaymentTypeId
        ) : IRequest<Result<int>>;
}
