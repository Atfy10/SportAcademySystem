using MediatR;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.SubscriptionTypeCommands.CreateSubscriptionType
{
    public record CreateSubscriptionTypeCommand(
        SubType Name,
        int DaysPerMonth,
        bool IsActive,
        bool IsOffer
        ) : IRequest<Result<int>>;

}
