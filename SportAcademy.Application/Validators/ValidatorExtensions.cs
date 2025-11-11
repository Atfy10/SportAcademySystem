using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
