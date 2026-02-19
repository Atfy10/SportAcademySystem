using MediatR;
using SportAcademy.Application.Common.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.DeleteSubscriptionDetails
{
    public record DeleteSubscriptionDetailsCommand(
        int Id
    ) : IRequest<Result<bool>>;
}
