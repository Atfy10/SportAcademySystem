using FluentValidation;
using SportAcademy.Application.Queries.SessionOccurrenceQueries.GetById;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.SessionOccurrenceValidators
{
    public class GetSessionOccurrenceByIdValidator : AbstractValidator<GetSessionOccurrenceByIdQuery>
    {
        public GetSessionOccurrenceByIdValidator()
        {
            RuleFor(x => x.Id)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0)
                .WithMessage("Please provide a valid SessionOccurrence ID (must be greater than zero).");
        }
    }
}
