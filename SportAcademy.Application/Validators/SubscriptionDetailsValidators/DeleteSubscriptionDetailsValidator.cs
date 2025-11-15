using FluentValidation;
using SportAcademy.Application.Commands.SubscriptionDetailsCommands.DeleteSubscriptionDetails;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.SubscriptionDetailsValidators
{
    public class DeleteSubscriptionDetailsValidator : AbstractValidator<DeleteSubscriptionDetailsCommand>
    {
        public DeleteSubscriptionDetailsValidator()
        {
            RuleFor(x => x.Id).ApplyIdRuleFor("Subscription Details");
        }
    }
}
