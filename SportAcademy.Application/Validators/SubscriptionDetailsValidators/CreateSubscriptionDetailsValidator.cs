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
                .NotEmpty().WithMessage("Start date is required.");

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate)
                .WithMessage("End date must be after start date.");

            RuleFor(x => x.PaymentNumber)
                .NotEmpty().WithMessage("Payment number is required.");

            RuleFor(x => x.TraineeId)
                .GreaterThan(0).WithMessage("Invalid trainee ID.");

            RuleFor(x => x.SubscriptionTypeId)
                .GreaterThan(0).WithMessage("Invalid subscription type ID.");
        }
    }
}
