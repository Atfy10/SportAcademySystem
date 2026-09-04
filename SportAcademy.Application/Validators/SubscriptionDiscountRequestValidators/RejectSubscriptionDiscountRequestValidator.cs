using FluentValidation;
using SportAcademy.Application.Commands.SubscriptionDiscountRequestCommands.RejectSubscriptionDiscountRequest;

namespace SportAcademy.Application.Validators.SubscriptionDiscountRequestValidators
{
    public class RejectSubscriptionDiscountRequestValidator : AbstractValidator<RejectSubscriptionDiscountRequestCommand>
    {
        public RejectSubscriptionDiscountRequestValidator()
        {
            RuleFor(x => x.Id).ApplyIdRuleFor("Subscription discount request");

            RuleFor(x => x.RejectionReason)
                .NotEmpty().WithMessage("A rejection reason is required.")
                .MaximumLength(500).WithMessage("Rejection reason must not exceed 500 characters.");
        }
    }
}
