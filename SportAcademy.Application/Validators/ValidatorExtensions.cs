using FluentValidation;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;

namespace SportAcademy.Application.Validators
{
    internal static class ValidatorExtensions
    {
        /// <summary>Country-aware phone format check (libphonenumber, via
        /// IRegionalValidationService) using the current tenant's configured country. Empty is
        /// treated as valid here - pair with a separate NotEmpty() when the field is required,
        /// same convention every other rule in this codebase follows.</summary>
        public static IRuleBuilderOptions<T, string?> ApplyPhoneRuleFor<T>(
            this IRuleBuilder<T, string?> ruleBuilder,
            IRegionalValidationService regionalValidation,
            ITenantSettingsCountryReader countryReader)
        {
            return ruleBuilder
                .MustAsync(async (phone, ct) =>
                {
                    if (string.IsNullOrWhiteSpace(phone))
                        return true;
                    var country = await countryReader.GetCountryAsync(ct);
                    return regionalValidation.IsValidPhoneNumber(phone, country);
                })
                .WithMessage("{PropertyName} is not a valid phone number for the configured region.");
        }

        /// <summary>Country-aware National ID format check (CountryRegionalRegistry, via
        /// IRegionalValidationService) using the current tenant's configured country.
        /// <paramref name="birthDateSelector"/> supplies the birth date for countries whose
        /// rule cross-checks it (Kuwait today) - pass a selector returning null when the entity
        /// being validated has no birth date to check against.</summary>
        public static IRuleBuilderOptions<T, string?> ApplyNationalIdRuleFor<T>(
            this IRuleBuilder<T, string?> ruleBuilder,
            IRegionalValidationService regionalValidation,
            ITenantSettingsCountryReader countryReader,
            Func<T, DateOnly?> birthDateSelector)
        {
            return ruleBuilder
                .MustAsync(async (instance, nationalId, ct) =>
                {
                    var country = await countryReader.GetCountryAsync(ct);
                    return regionalValidation.IsValidNationalId(nationalId, country, birthDateSelector(instance));
                })
                .WithMessage("{PropertyName} is not a valid National ID for the configured region.");
        }
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
