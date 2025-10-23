using FluentValidation;
using SportAcademy.Application.Queries.SportQueries.GetById;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.SportValidators
{
    public class GetSportByIdValidator : AbstractValidator<GetSportByIdQuery>
    {
        public GetSportByIdValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Please provide a sport ID.")
                .GreaterThan(0).WithMessage("Please provide a valid sport ID.");
        }
    }
}
