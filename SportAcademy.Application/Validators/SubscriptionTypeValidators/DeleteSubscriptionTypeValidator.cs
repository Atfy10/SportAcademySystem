using FluentValidation;
using SportAcademy.Application.Commands.SubscriptionTypeCommands.DeleteSubscriptionType;

namespace SportAcademy.Application.Validators.SubscriptionTypeValidators
{
    public class DeleteSubscriptionTypeValidator : AbstractValidator<DeleteSubscriptionTypeCommand>
    {
        public DeleteSubscriptionTypeValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id must be greater than 0.");
        }
    }
}