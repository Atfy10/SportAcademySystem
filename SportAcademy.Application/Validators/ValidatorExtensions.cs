using FluentValidation;

namespace SportAcademy.Application.Validators
{
    internal static class ValidatorExtensions
    {
        public static IRuleBuilderOptions<T, int> ApplyIdRuleFor<T>(
            this IRuleBuilderInitial<T, int> ruleBuilder,
            string entityName)
        {
            return ruleBuilder
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage($"Please choose a {entityName}.")
                .GreaterThan(0)
                .WithMessage($"Selected {entityName} is not valid.");
        }

        public static IRuleBuilderOptions<T, int?> ApplyOptionalIdRuleFor<T>(
            this IRuleBuilderInitial<T, int?> ruleBuilder,
            string entityName)
        {
            return ruleBuilder
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0)
                .WithMessage($"Selected {entityName} is not valid.")
                .When((x, value) => value != null);
        }

        public static IRuleBuilderOptions<T, string?> NoDigits<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                // Null/empty is not this rule's concern (NotEmpty handles that separately) - and
                // without the parens, `||` let the right-hand `value.Any(...)` still run against
                // a null value, throwing ArgumentNullException instead of just passing.
                .Must(value => string.IsNullOrEmpty(value) || !value.Any(char.IsDigit))
                .WithMessage("{PropertyName} must not contain digits.");
        }
    }
}
