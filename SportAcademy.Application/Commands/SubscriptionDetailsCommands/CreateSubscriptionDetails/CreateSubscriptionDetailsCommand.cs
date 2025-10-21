using MediatR;
using SportAcademy.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.CreateSubscriptionDetails
{
    public record CreateSubscriptionDetailsCommand(
        int SubscriptionTypeId,
        int TraineeId,
        DateOnly StartDate,
        DateOnly EndDate,
        bool IsActive,
        string PaymentNumber
        ) : IRequest<Result<int>>;
  
}
