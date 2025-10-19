using FluentValidation;
using SportAcademy.Application.Commands.BranchCommands.UpdateBranch;
using SportAcademy.Application.Commands.SubscriptionType.CreateSubscriptionType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.subscriptionTypeValidators
{
    public class CreateSubscriptionTypeValidator : AbstractValidator<CreateSubscriptionTypeCommand>
    {
        public CreateSubscriptionTypeValidator()
        {
            RuleFor(x => x.Name)
                .IsInEnum()
                .WithMessage("Invalid subscription type name. Must be one of the defined SubType values.");
            RuleFor(x => x.DaysPerMonth)
               .GreaterThan(0)
               .WithMessage("Days per month must be greater than zero.");
        }
    }
}
