using FluentValidation;
using SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetById;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.SubscriptionDetailsValidators
{
    public class GetSubDetailsByIdValidator : AbstractValidator<GetSubDetailsByIdQuery>
    {
        public GetSubDetailsByIdValidator()
        {
            RuleFor(x => x.Id)
                .ApplyIdRuleFor("Subscription Details");
        }
    }
}
