using FluentValidation;
using SportAcademy.Application.Commands.BranchCommands.UpdateBranch;
using SportAcademy.Application.Commands.SubscriptionTypeCommands.CreateSubscriptionType;
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
                .Cascade(CascadeMode.Stop)
                .IsInEnum()
                .WithMessage("Please select a valid subscription type from the list.");

            RuleFor(x => x.DaysPerMonth)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0)
                .WithMessage("Days per month must be greater than zero.")
                .LessThanOrEqualTo(12)
                .WithMessage("Days per month cannot exceed 12.");

            RuleFor(x => x.IsActive)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage("Please specify whether the subscription is active.");

            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .Must(x => !(!x.IsActive && x.IsOffer))
                .WithMessage("An offer cannot be inactive.")
                .NotNull()
                .WithMessage("Please specify if this is a special offer.");
        }
    }
}
