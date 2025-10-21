using FluentValidation;
using SportAcademy.Application.Commands.BranchCommands.AddSportToBranch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.BranchValidators
{
    public class AddSportToBranchCommandValidator : AbstractValidator<AddSportToBranchCommand>
    {
        public AddSportToBranchCommandValidator()
        {
            RuleFor(x => x.SportId)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("SportId must be greater than 0.");

            RuleFor(x => x.BranchId)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("BranchId must be greater than 0.");
        }
    }
}
