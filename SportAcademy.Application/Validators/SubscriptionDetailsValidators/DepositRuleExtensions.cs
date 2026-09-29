using FluentValidation;
using System.Linq.Expressions;

namespace SportAcademy.Application.Validators.SubscriptionDetailsValidators
{
    // Shared by the plain create command and the discount-request command so "pay a deposit"
    // means the same thing on both paths.
    public static class DepositRuleExtensions
    {
        public static void ApplyDepositRules<T>(
            this AbstractValidator<T> validator,
            Expression<Func<T, bool>> payDeposit,
            Expression<Func<T, decimal?>> depositAmount,
            Expression<Func<T, DateOnly?>> balanceDueDate)
        {
            var isDeposit = payDeposit.Compile();

            validator.RuleFor(depositAmount)
                .NotNull().WithMessage("Enter the deposit amount being paid now.")
                .GreaterThan(0).WithMessage("The deposit must be greater than zero.")
                .Must(a => a is null || decimal.Round(a.Value, 3) == a.Value)
                .WithMessage("The deposit can have at most 3 decimal places.")
                .When(x => isDeposit(x));

            validator.RuleFor(balanceDueDate)
                .NotNull().WithMessage("Choose the date the remaining balance will be collected.")
                .Must(d => d is null || d.Value >= DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("The collect date can't be in the past.")
                .When(x => isDeposit(x));

            validator.RuleFor(depositAmount)
                .Null().WithMessage("A deposit amount was sent without \"Pay a deposit\" being selected.")
                .When(x => !isDeposit(x));
        }
    }
}
