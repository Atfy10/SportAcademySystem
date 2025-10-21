using FluentValidation;
using SportAcademy.Application.Commands.SubscriptionDetailsCommands.CreateSubscriptionDetails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.SubscriptionDetailsValidators
{
    public class CreateSubscriptionDetailsValidator : AbstractValidator<CreateSubscriptionDetailsCommand>
    {
        public CreateSubscriptionDetailsValidator()
        {
            RuleFor(x => x.StartDate)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Please select a start date.")
                .WithMessage("Start date cannot be in the past.");

            RuleFor(x => x.EndDate)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(x => x.StartDate)
                .WithMessage("End date should be after the start date.");

            RuleFor(x => x.PaymentNumber)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Please enter the payment number.")
                .MaximumLength(20).WithMessage("");

            RuleFor(x => x.TraineeId)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Please choose a valid trainee.");

            RuleFor(x => x.SubscriptionTypeId)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Please select a valid subscription type.");
        }
    }
}
