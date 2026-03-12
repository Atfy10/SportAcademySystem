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
                .Must(value => value is not null && string.IsNullOrEmpty(value) || !value.Any(char.IsDigit))
                .WithMessage("{PropertyName} must not contain digits.");
        }
    }
}
