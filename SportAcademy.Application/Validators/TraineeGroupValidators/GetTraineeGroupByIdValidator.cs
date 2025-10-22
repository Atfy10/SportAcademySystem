using FluentValidation;
using SportAcademy.Application.Queries.TraineeGroupQueries.GetById;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.TraineeGroupValidators
{
    public class GetTraineeGroupByIdValidator : AbstractValidator<GetTraineeGroupByIdQuery>
    {
        public GetTraineeGroupByIdValidator()
        {
            RuleFor(x => x.Id)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Please provide the trainee group ID.")
                .GreaterThan(0).WithMessage("Trainee group ID must be a valid positive number.");
        }
    }
}
