using FluentValidation;
using SportAcademy.Application.Queries.EnrollmentQueries.GetById;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.EnrollmentValidators
{
    public class GetEnrollmentByIdValidator : AbstractValidator<GetEnrollmentByIdQuery>
    {
        public GetEnrollmentByIdValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Please provide an enrollment ID.")
                .GreaterThan(0).WithMessage("Please provide a valid enrollment ID.");
        }
    }
}
