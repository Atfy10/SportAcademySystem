using FluentValidation;
using SportAcademy.Application.Commands.SportPriceCommands.UpdateSportPrice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.SportPriceValidators
{
    public class UpdateSportPriceValidators : AbstractValidator<UpdateSportPriceCommand>
    {
        public UpdateSportPriceValidators()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.SportId)
                .GreaterThan(0)
                .WithMessage("Please select a valid sport.");

            RuleFor(x => x.BranchId)
                .GreaterThan(0)
                .WithMessage("Please select a valid branch.");

            RuleFor(x => x.SubsTypeId)
                .GreaterThan(0)
                .WithMessage("Please select a valid subscription type.");

            RuleFor(x => x.NewPrice)
                .GreaterThan(0)
                .WithMessage("Price must be greater than zero.")
                .LessThanOrEqualTo(10_000)
                .WithMessage("Price seems too high. Please check the value again.");
        }
    }
}
