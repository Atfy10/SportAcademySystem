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
                .GreaterThan(0)
                .WithMessage($"Please provide a valid {entityName} ID (must be greater than zero).");
        }
    }
}
